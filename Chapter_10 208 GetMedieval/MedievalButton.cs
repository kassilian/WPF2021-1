using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
namespace GetMedieval
{ 
    public class MedievalButton : Control 
    {   
        // Два приватных элемента
        FormattedText formtxt;        
        bool isMouseReallyOver;        
        
        // Статические поля, доступные только для чтения        
        public static readonly DependencyProperty  TextProperty;        
        public static readonly RoutedEvent KnockEvent;        
        public static readonly RoutedEvent  PreviewKnockEvent;        
        
        // Статический конструктор         
        static MedievalButton()         
        {         
            // Регистрация зависимого свойства         
            TextProperty = DependencyProperty.Register("Text", typeof(string), typeof (MedievalButton), new FrameworkPropertyMetadata(" ", FrameworkPropertyMetadataOptions.AffectsMeasure));        
            
            // Регистрация маршрутизируемых событий         
            KnockEvent = EventManager.RegisterRoutedEvent ("Knock", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MedievalButton));     
            PreviewKnockEvent = EventManager.RegisterRoutedEvent ("PreviewKnock", RoutingStrategy.Tunnel, typeof(RoutedEventHandler), typeof(MedievalButton));    
        }
        
        // Открытый интерфейс к зависимому свойству (property)   
        public string Text   
        {   
            set { SetValue(TextProperty, value == null  ? " " : value); }        
            get { return (string)GetValue(TextProperty); }     
        }    
        
        // Открытый интерфейс к маршрутизируемым событиям      
        public event RoutedEventHandler Knock    
        {        
            add { AddHandler(KnockEvent, value); }       
            remove { RemoveHandler(KnockEvent, value); }   
        }    
        
        public event RoutedEventHandler PreviewKnock    
        {        
            add { AddHandler(PreviewKnockEvent, value); }        
            remove { RemoveHandler(PreviewKnockEvent,  value); }    
        }     
        
        // MeasureOverride метод вызывется при возможном изменении размеров кнопки
        protected override Size MeasureOverride(Size  sizeAvailable)    
        {
            formtxt = new FormattedText(Text, CultureInfo.CurrentCulture, FlowDirection, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize, Foreground);
            
            // Внутринние отступы учитываются при вычислении размера    
            Size sizeDesired = new Size(Math.Max(48,  formtxt.Width) + 4, formtxt.Height + 4);        
            sizeDesired.Width += Padding.Left +  Padding.Right;        
            sizeDesired.Height += Padding.Top +  Padding.Bottom;        
            return sizeDesired;    
        }     
        
        // Метод OnRender вызывается при перерисовки кнопки    
        protected override void OnRender (DrawingContext dc)    
        {        
            
            // Определение цвета фона
            Brush brushBackground = SystemColors .ControlBrush;             
            if (isMouseReallyOver && IsMouseCaptured)               
                brushBackground = SystemColors .ControlDarkBrush;            
            
            // Определение ширины пера           
            Pen pen = new Pen(Foreground,  IsMouseOver ? 2 : 1);           
            
            // Нарисовать прямоугольник со скруглёнными углами             
            dc.DrawRoundedRectangle (brushBackground, pen, new Rect(new  Point(0, 0), RenderSize), 4, 4);          
            
            // Определение основного цвета 
            formtxt.SetForegroundBrush( IsEnabled ? Foreground :  SystemColors.ControlDarkBrush);          
            
            // Определение начальной точки текста            
            Point ptText = new Point(2, 2);            
            switch (HorizontalContentAlignment)           //  определение для горизонтального положения
            {                
                case HorizontalAlignment.Left:                  
                    ptText.X += Padding.Left;                 
                    break;               
                case HorizontalAlignment.Right:       
                    ptText.X += RenderSize.Width - formtxt.Width - Padding.Right;   
                    break;               
                case HorizontalAlignment.Center:            
                case HorizontalAlignment.Stretch:                
                    ptText.X += (RenderSize.Width - formtxt.Width - Padding.Left - Padding.Right) / 2;    
                    break;           
            }            
            switch (VerticalContentAlignment)           //  определение для вертикального положения
            {                
                case VerticalAlignment.Top:        
                    ptText.Y += Padding.Top;            
                    break;            
                case VerticalAlignment.Bottom:  
                    ptText.Y += RenderSize.Height - formtxt.Height - Padding.Bottom;    
                    break;             
                case VerticalAlignment.Center:      
                case VerticalAlignment.Stretch:      
                    ptText.Y += (RenderSize.Height - formtxt.Height - Padding.Top - Padding.Bottom) / 2;                  
                    break;          
            }             
            
            // Вывод текста
            dc.DrawText(formtxt, ptText);      
        }       
        // Событие мыши, влияющие на внешний вид кнопки      
        protected override void OnMouseEnter (MouseEventArgs args)      
        {      
            base.OnMouseEnter(args);         
            InvalidateVisual();        
        }       
        protected override void OnMouseLeave (MouseEventArgs args)  
        {          
            base.OnMouseLeave(args);          
            InvalidateVisual();       
        }      
        protected override void OnMouseMove (MouseEventArgs args)    
        {             base.OnMouseMove(args); 
            
            // Проверка направления движения       
            Point pt = args.GetPosition(this);   
            bool isReallyOverNow = (pt.X >= 0 && pt.X < ActualWidth && pt.Y >= 0 &&  pt.Y < ActualHeight);         
            if (isReallyOverNow != isMouseReallyOver)       
            {             
                isMouseReallyOver = isReallyOverNow;        
                InvalidateVisual();          
            }      
        }

        // Событие при котором начинается "Knock"
        protected override void  OnMouseLeftButtonDown(MouseButtonEventArgs args)   
        {         
            base.OnMouseLeftButtonDown(args);         
            CaptureMouse();       
            InvalidateVisual();      
            args.Handled = true;        
        }

        // Конкретно инициализация "Knock"       
        protected override void  OnMouseLeftButtonUp(MouseButtonEventArgs args)   
        {       
            base.OnMouseLeftButtonUp(args);     
            if (IsMouseCaptured)           
            {          
                if (isMouseReallyOver)          
                {                   
                    OnPreviewKnock();          
                    OnKnock();             
                }                
                args.Handled = true;    
                Mouse.Capture(null);   
            }        
        }      
        
        // При потере захвата мыши кнопка перерисовывается    
        protected override void OnLostMouseCapture (MouseEventArgs args)    
        {       
            base.OnLostMouseCapture(args);         
            InvalidateVisual();      
        } 
        
        // Клавиши "пробел" или Enter также вызывают срабатывание кнопки    
        protected override void OnKeyDown (KeyEventArgs args)     
        {       
            base.OnKeyDown(args);   
            if (args.Key == Key.Space || args.Key  == Key.Enter)              
                args.Handled = true;      
        }       
        protected override void OnKeyUp (KeyEventArgs args)   
        {       
            base.OnKeyUp(args);       
            if (args.Key == Key.Space || args.Key  == Key.Enter)         
            {              
                OnPreviewKnock();          
                OnKnock();         
                args.Handled = true;       
            }        
        }       
        
        // Метод "OnKnock" инициализирует событие "Knock"        
        protected virtual void OnKnock()        
        {           
            RoutedEventArgs argsEvent = new  RoutedEventArgs();     
            argsEvent.RoutedEvent = MedievalButton .PreviewKnockEvent;      
            argsEvent.Source = this;          
            RaiseEvent(argsEvent);   
        }      
        
        // Метод OnPreviewKnock инициирует событие "PreviewKnock"      
        protected virtual void OnPreviewKnock()    
        {       
            RoutedEventArgs argsEvent = new RoutedEventArgs();   
            argsEvent.RoutedEvent = MedievalButton .KnockEvent;    
            argsEvent.Source = this;      
            RaiseEvent(argsEvent);        
        }   
    }
} 