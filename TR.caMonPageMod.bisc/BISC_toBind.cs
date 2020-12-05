using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace TR.caMonPageMod.bisc
{
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

				if (ConnectToBVE == true && caMon.SharedFuncs.SML.PanelA.Length > value)
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

		private ObservableCollection<IBISCCtrl> __Items = null;
		public ObservableCollection<IBISCCtrl> Items
		{
			get => __Items;
			set
			{
				__Items = value;
				OnPropertyChanged(nameof(Items));
			}
		}

		private Visibility __CollapsedWhenDrawing = Visibility.Visible;
		public Visibility CollapsedWhenDrawing
		{
			get => __CollapsedWhenDrawing;
			set
			{
				__CollapsedWhenDrawing = value;
				OnPropertyChanged(nameof(CollapsedWhenDrawing));
			}
		}

	}
}
