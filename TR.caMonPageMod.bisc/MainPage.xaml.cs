using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

namespace TR.caMonPageMod.bisc
{
	/// <summary>
	/// MainPage.xaml の相互作用ロジック
	/// </summary>
	public partial class MainPage : Page, caMon.IPages
	{
		static readonly string System_Drawimg_Common_Dll_Path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "System.Drawimg.Common.dll");
		const string System_Drawing_Common_Dll_Fullname = @"System.Drawing.Common, Version=5.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";
		private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			//dllを自動で見つけられなかった場合
			switch (args.Name)
			{
				case System_Drawing_Common_Dll_Fullname:
					if (File.Exists(System_Drawimg_Common_Dll_Path) == true)
					{
						Assembly a = Assembly.LoadFrom(System_Drawimg_Common_Dll_Path);
						if (string.Equals(System_Drawing_Common_Dll_Fullname, a?.FullName))
							return a;
					}
					return null;
				default:
					return null;
			}
		}

		public MainPage()
		{
			InitializeComponent();

			DataContext = b2b;
		}

		public Page FrontPage { get => this; }

		public event EventHandler BackToHome;
		public event EventHandler CloseApp;

		public void Dispose() { }
		BISC_toBind b2b = new BISC_toBind();

		private void OpenFile(object sender = null, RoutedEventArgs e = null)
		{
			//ref : https://johobase.com/wpf-file-folder-common-dialog/
			var dialog = new OpenFileDialog();

			//ref : https://docs.microsoft.com/ja-jp/dotnet/api/system.windows.controls.image?view=net-5.0
			dialog.Filter = "画像ファイル(bmp, gif, ico, jpg, jpeg, png, wdp, tiff)|*.bmp;*.gif;*.ico;*.jpg;*.jpeg;*.png;*.wdp;*.tiff|全てのファイル|*.*";
			dialog.CheckFileExists = true;
			dialog.Multiselect = false;
			
			if (dialog.ShowDialog() == true)
			{
				var bisc = new BISCCtrl();
				bisc.MouseDown += BISC_MouseDown;
				bisc.Source = new BitmapImage(new Uri(dialog.FileName));
				bisc.SetBinding(BISCCtrl.ValueToShowProperty, "CurrentValue");
				b2b.BISC = bisc;
			}
		}

		private void AddBtnClicked(object sender = null, RoutedEventArgs e = null)
		{
			if (b2b.BISC == null)
				return;

			if (PreviewGrid.Children?.Contains(b2b.BISC) == false) PreviewGrid.Children.Add(b2b.BISC);

			b2b.Items ??= new ObservableCollection<IBISCCtrl>();
			if (b2b.Items?.Contains(b2b.BISC) != true) b2b.Items.Add(b2b.BISC);

			b2b.BISC = null;
		}

		private void BISC_MouseDown(object sender, MouseButtonEventArgs e)
			=> b2b.BISC = sender as BISCCtrl;

		private void Page_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.F1:
					OpenFile();
					break;

				case Key.F3:
					RmvBtnClicked();
					break;
				case Key.F4:
					AddBtnClicked();
					break;
			}
		}

		private void RmvBtnClicked(object sender = null, RoutedEventArgs e = null)
		{
			if (b2b.BISC == null)
				return;
			var bisc = b2b.BISC;
			if (PreviewGrid.Children?.Contains(bisc) == true) PreviewGrid.Children.Remove(bisc);
			if (b2b.Items?.Contains(bisc) == true) b2b.Items.Remove(bisc);

			b2b.BISC = null;
		}

		private void SaveBtnClicked(object sender, RoutedEventArgs e)
		{
			int imgHeight = 0;
			int imgWidth = 0;
			int bitmask = 0;
			byte[][] px2w;
			const int BYTES_PER_PIXEL = 4;//BGRA

			if (b2b.Items?.Count is null or <= 0)
			{
				_ = MessageBox.Show("There are no Image!");
				return;
			}


			#region 出力画像サイズの確認
			foreach (var v in b2b.Items)
				if (v.ShowWhen0 || v.ShowWhen1)
					bitmask |= 0x1 << v.BitPositionNumber;//必要な最大値を確認

			if (((long)bitmask * b2b.Interval) >= Int32.MaxValue)//Int32.MaxValue以上は非対応
			{
				_ = MessageBox.Show("必要な画像高さが大きすぎます.\n本ソフトウェアでは, Int32.MaxValue以上の高さの画像を扱えません.");
				return;
			}

			#region 2のn乗確認
			imgHeight = bitmask * b2b.Interval;
			imgWidth = (int)Math.Ceiling(PreviewGrid.ActualWidth);//小数点以下切り上げ
			int tmp_w = 0;
			int tmp_h = 0;
			for (int i = 0; i < 32; i++)
				if (imgHeight <= (0x1 << i))
				{
					tmp_h = 0x1 << i;
					break;
				}
			for (int i = 0; i < 32; i++)
				if (imgWidth <= (0x1 << i))
				{
					tmp_w = 0x1 << i;
					break;
				}

			if (imgHeight != tmp_h && imgWidth != tmp_w && MessageBox.Show("高さ/幅を2のn乗にしますか?", "Set the height/width to 2^n value?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
			{
				imgHeight = tmp_h;
				imgWidth = tmp_w;
			}

			#endregion 2のn乗確認
			#endregion 出力画像サイズの確認

			var dig = new SaveFileDialog();
			dig.Filter = "PNG(*.png)|*.png";//現状はpngのみ対応

			if (dig.ShowDialog() == true)
			{
				#region 全描画画像取得
				b2b.CollapsedWhenDrawing = Visibility.Collapsed;//UI描画コストを減らすため
				bool? BIDSConnection = b2b.ConnectToBVE;

				px2w = new byte[bitmask + 1][];
				RenderTargetBitmap rtb = new RenderTargetBitmap(imgWidth, b2b.Interval, 96, 96, PixelFormats.Pbgra32);
				for (int i = 0; i <= bitmask; i++)
				{
					b2b.CurrentValue = i;
					px2w[i] = new byte[imgWidth * b2b.Interval * BYTES_PER_PIXEL];
					rtb.Render(PreviewGrid);//念のため
					rtb.CopyPixels(px2w[i], imgWidth * BYTES_PER_PIXEL, 0);
					rtb.Clear();
				}

				b2b.ConnectToBVE = BIDSConnection;
				b2b.CollapsedWhenDrawing = Visibility.Visible;//UI描画コストを減らすため 復旧
				#endregion 全描画画像取得
				try
				{
					using (Bitmap img = new Bitmap(imgWidth, imgHeight))
					{
						for (int i = 0; i <= bitmask; i++)
						{
							BitmapData bd = null;
							if (px2w[i]?.Length is null or <= 0)
								return;
							try
							{
								bd = img.LockBits(new Rectangle(0, i * b2b.Interval, imgWidth, b2b.Interval), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
								var intptr = bd.Scan0;
								Marshal.Copy(px2w[i], 0, intptr, px2w[i].Length);
								px2w[i] = null;//はやく解放してもらうために
							}
							finally
							{
								if (bd is not null) img.UnlockBits(bd);
							}
						}



						img.Save(dig.FileName,
							Path.GetExtension(dig.FileName).ToLower() switch
							{
								".png" => ImageFormat.Png,
								".jpg" => ImageFormat.Jpeg,
								".jpeg" => ImageFormat.Jpeg,
								".bmp" => ImageFormat.Bmp,
								_ => ImageFormat.Png
							});
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("画像生成/保存処理に失敗しました.\n" + ex.ToString());
					return;
				}
			}
		}

		private void CurrentValueTB_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.Up:
					b2b.CurrentValue++;
					break;
				case Key.Down:
					if (b2b.CurrentValue > 0)
						b2b.CurrentValue--;
					break;
			}
		}
	}
}
