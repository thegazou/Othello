using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Othello_graphique
{
    class Tile : Button, INotifyPropertyChanged
    {

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void FirePropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion

        private MainWindow parent;
        private int pion;

        public int Pion
        {
            get { return pion; }
            set
            {
                if(pion != value)
                {
                    pion = value;
                    if (pion == 1)
                    {
                        Ellipse circle = new Ellipse();
                        circle.Width = this.ActualWidth * 0.9;
                        circle.Height = this.ActualHeight * 0.9;
                        circle.Fill = Brushes.Black;
                        circle.Stroke = Brushes.Black;
                        Content = circle;
                    }
                    else if (pion == -1)
                    {
                        Ellipse circle = new Ellipse();
                        circle.Width = this.ActualWidth * 0.9;
                        circle.Height = this.ActualHeight * 0.9;
                        circle.Fill = Brushes.White;
                        circle.Stroke = Brushes.White;
                        Content = circle;
                    }
                    else
                    {
                        Content = "";
                    }
                    this.FirePropertyChanged("Pion");
                }
            }
        }

        private int row;
        public int Row
        {
            get { return row; }
        }

        private int col;
        public int Col
        {
            get { return col; }
        }

        public Tile(MainWindow parent, int row, int col)
        {
            this.parent = parent;
            this.row = row;
            this.col = col;
            Background = new LinearGradientBrush(Colors.LightGray, Colors.LightGray, 90);
            this.Click += parent.Tile_Click;
        }

        public void changeBackground(bool isPlayable)
        {
            if (isPlayable)
            {
                Background = new LinearGradientBrush(Colors.Gray, Colors.Gray, 90);
            }
            else
            {
                Background = new LinearGradientBrush(Colors.LightGray, Colors.LightGray, 90);
            }
        }

    }
}