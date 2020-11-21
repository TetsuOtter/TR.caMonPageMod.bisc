using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

using Microsoft.Win32;

namespace TR.caMonPageMod.bisc
{
	/// <summary>
	/// MainPage.xaml の相互作用ロジック
	/// </summary>
	public partial class MainPage : Page, caMon.IPages
	{
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

			b2b.Items ??= new ObservableCollection<BISCCtrl>();
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
			MessageBox.Show("Save!");
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
	public class BISC_toBind : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		private void OnPropertyChanged(string s) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(s));

		public BISC_toBind()
		{
			caMon.SharedFuncs.SML.SMC_PanelDChanged += SML_SMC_PanelDChanged;
		}

		private void SML_SMC_PanelDChanged(object sender, ValueChangedEventArgs<int[]> e)
		{
			if (ConnectToBVE == true && caMon.SharedFuncs.SML.PanelA.Length > PanelIndex)
				CurrentValue = caMon.SharedFuncs.SML.PanelA[PanelIndex];
		}

		private BISCCtrl __BISC = null;
		public BISCCtrl BISC
		{
			get => __BISC;
			set
			{
				__BISC = value;
				MarginL = BISC?.MyMargin.Left ?? 0;
				MarginT = BISC?.MyMargin.Top ?? 0;
				OnPropertyChanged(nameof(BISC));
			}
		}

		private double __MarginL = 0;
		public double MarginL
		{
			get => __MarginL;
			set
			{
				__MarginL = value;
				if (BISC is not null)
				{
					Thickness t = BISC.MyMargin;
					t.Left = value;
					BISC.MyMargin = t;
				}
				OnPropertyChanged(nameof(MarginL));
			}
		}

		private double __MarginT = 0;
		public double MarginT
		{
			get => __MarginT;
			set
			{
				__MarginT = value;
				if (BISC is not null)
				{
					Thickness t = BISC.MyMargin;
					t.Top = value;
					BISC.MyMargin = t;
				}
				OnPropertyChanged(nameof(MarginT));
			}
		}

		private int __CurrentValue = 0;
		public int CurrentValue
		{
			get => __CurrentValue;
			set
			{
				__CurrentValue = value;
				OnPropertyChanged(nameof(CurrentValue));
			}
		}

		private bool? __ConnectToBVE = false;
		public bool? ConnectToBVE
		{
			get => __ConnectToBVE;
			set
			{
				__ConnectToBVE = value;
				OnPropertyChanged(nameof(ConnectToBVE));

			}
		}

		private int __PanelIndex = 0;
		public int PanelIndex
		{
			get => __PanelIndex;
			set
			{
				__PanelIndex = value;
				OnPropertyChanged(nameof(PanelIndex));

				if (ConnectToBVE ==true && caMon.SharedFuncs.SML.PanelA.Length > value)
					CurrentValue = caMon.SharedFuncs.SML.PanelA[value];
			}
		}

		private int __Interval = 4;
		public int Interval
		{
			get => __Interval;
			set
			{
				__Interval = value;
				OnPropertyChanged(nameof(Interval));
			}
		}

		private ObservableCollection<BISCCtrl> __Items = null;
		public ObservableCollection<BISCCtrl> Items
		{
			get => __Items;
			set
			{
				__Items = value;
				OnPropertyChanged(nameof(Items));
			}
		}
	}
}
