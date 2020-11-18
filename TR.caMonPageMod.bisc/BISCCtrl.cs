using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TR.caMonPageMod.bisc
{
	public class BISCCtrl : Control
	{
		static public DependencyProperty SourceProperty = DependencyProperty.Register(nameof(Source), typeof(BitmapSource), typeof(BISCCtrl));
		public BitmapSource Source
		{
			get => (BitmapSource)GetValue(SourceProperty);
			set => SetValue(SourceProperty, value);
		}

		static public DependencyProperty BitPositionNumberProperty = DependencyProperty.Register(nameof(BitPositionNumber), typeof(int), typeof(BISCCtrl), new PropertyMetadata(0, PropChanged));
		public int BitPositionNumber
		{
			get => (int)GetValue(BitPositionNumberProperty);
			set => SetValue(BitPositionNumberProperty, value);
		}
		
		static public DependencyProperty ValueToShowProperty = DependencyProperty.Register(nameof(ValueToShow), typeof(int), typeof(BISCCtrl), new PropertyMetadata(0, PropChanged));
		public int ValueToShow
		{
			get => (int)GetValue(ValueToShowProperty);
			set => SetValue(ValueToShowProperty, value);
		}

		static public DependencyProperty ShowWhen0Property = DependencyProperty.Register(nameof(ShowWhen0), typeof(bool), typeof(BISCCtrl), new PropertyMetadata(false, PropChanged));
		public bool ShowWhen0
		{
			get => (bool)GetValue(ShowWhen0Property);
			set => SetValue(ShowWhen0Property, value);
		}

		static public DependencyProperty ShowWhen1Property = DependencyProperty.Register(nameof(ShowWhen1), typeof(bool), typeof(BISCCtrl), new PropertyMetadata(true, PropChanged));
		public bool ShowWhen1
		{
			get => (bool)GetValue(ShowWhen1Property);
			set => SetValue(ShowWhen1Property, value);
		}

		static private void PropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => (d as BISCCtrl)?.SetVisibility();


		static public DependencyProperty CurrentVisibilityProperty = DependencyProperty.Register(nameof(CurrentVisibility), typeof(Visibility), typeof(BISCCtrl));
		public Visibility CurrentVisibility
		{
			get => (Visibility)GetValue(CurrentVisibilityProperty);
			private set => SetValue(CurrentVisibilityProperty, value);
		}


		static BISCCtrl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(BISCCtrl), new FrameworkPropertyMetadata(typeof(BISCCtrl)));


		private Visibility GetVisibility(int value)
			=> value switch
			{
				0 => ShowWhen0 ? Visibility.Visible : Visibility.Hidden,
				1 => ShowWhen1 ? Visibility.Visible : Visibility.Hidden,
				_ => Visibility.Hidden
			};
		private void SetVisibility() => CurrentVisibility = ValueToShow < 0 ? Visibility.Hidden : GetVisibility((ValueToShow >> BitPositionNumber) & 1);
		

	}
}
