using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Othello_graphique
{
    class Tile : Button, INotifyPropertyChanged
    {

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                MessageBox.Show(PropertyChanged.ToString());

                if (this.pion == 1)
                {
                    Ellipse circle = new Ellipse();
                    circle.Width = this.ActualWidth * 0.9;
                    circle.Height = this.ActualHeight * 0.9;
                    circle.Fill = Brushes.Black;
                    circle.Stroke = Brushes.Black;
                    Content = circle;
                }
                else if (this.pion == -1)
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

                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

        private MainWindow parent;
        private int pion;

        /// <summary>
        /// Get the actual value of the tile
        /// </summary>
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

                    OnPropertyChanged("Pion");
                }
                
            }
        }

        private int row;

        /// <summary>
        /// Get the row of the tile
        /// </summary>
        public int Row
        {
            get { return row; }
        }

        private int col;
        /// <summary>
        /// Get the column of the tile
        /// </summary>
        public int Col
        {
            get { return col; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">get the main window</param>
        /// <param name="row">get the row of the tile 0 to 7</param>
        /// <param name="col">get the column of the tile 0 to 7</param>
        /// <returns></returns>
        public Tile(MainWindow parent, int row, int col)
        {
            this.parent = parent;
            this.row = row;
            this.col = col;
            Background = new LinearGradientBrush(Colors.LightGray, Colors.LightGray, 90);
            this.Click += parent.Tile_Click;
        }

        /// <summary>
        /// Will change the background color if the tile is playable
        /// </summary>
        /// <param name="isPlayable">true if the tile is playable, false if it is not playable</param>
        /// <returns></returns>
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


        /// <summary>
        /// Set the binding.
        /// </summary>
        internal void SetBinding(int pion, Binding bindingBoard)
        {
           this.Pion = (int)bindingBoard.Source;
        }

    }
}