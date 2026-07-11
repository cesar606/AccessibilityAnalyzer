// Treball de Fi de Grau - Cesar Gallardo Rodriguez

namespace AccessibilityAnalyzer.App.Controls
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Automation;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Displays an accessibility score as a circular gauge.
    /// </summary>
    public partial class ScoreGauge : UserControl
    {
        /// <summary>
        /// Identifies the <see cref="Score"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ScoreProperty =
            DependencyProperty.Register(
                nameof(Score),
                typeof(int),
                typeof(ScoreGauge),
                new PropertyMetadata(0, OnScoreChanged));

        private const double Radius = 82.0;
        private const double CentreX = 90.0;
        private const double CentreY = 90.0;

        /// <summary>
        /// Initialises a new instance of the <see cref="ScoreGauge"/> class.
        /// </summary>
        public ScoreGauge()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the score to display, from 0 to 100.
        /// </summary>
        public int Score
        {
            get => (int)this.GetValue(ScoreProperty);
            set => this.SetValue(ScoreProperty, value);
        }

        /// <summary>
        /// Redraws the gauge whenever the score changes.
        /// </summary>
        /// <param name="d">The control whose score changed.</param>
        /// <param name="e">The event data.</param>
        private static void OnScoreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScoreGauge gauge)
            {
                gauge.Redraw();
            }
        }

        /// <summary>
        /// Returns the colour matching the score, so that the result is also conveyed
        /// by the number itself and not by colour alone.
        /// </summary>
        /// <param name="score">The score to evaluate.</param>
        /// <returns>The brush to paint the arc with.</returns>
        private static Brush GetScoreBrush(int score)
        {
            if (score >= 80)
            {
                return new SolidColorBrush(Color.FromRgb(0x1E, 0x7E, 0x45));
            }

            if (score >= 50)
            {
                return new SolidColorBrush(Color.FromRgb(0xB5, 0x65, 0x1A));
            }

            return new SolidColorBrush(Color.FromRgb(0xA5, 0x28, 0x1B));
        }

        /// <summary>
        /// Draws the arc corresponding to the current score.
        /// </summary>
        private void Redraw()
        {
            this.ScoreText.Text = string.Format(CultureInfo.InvariantCulture, "{0}%", this.Score);
            this.ProgressArc.Stroke = GetScoreBrush(this.Score);

            // The score is also announced to assistive technologies, so that it does not
            // rely on the visual gauge alone.
            AutomationProperties.SetName(
                this,
                string.Format(CultureInfo.InvariantCulture, "Puntuació d'accessibilitat: {0} per cent", this.Score));

            if (this.Score <= 0)
            {
                this.ProgressArc.Data = null;
                return;
            }

            // A full circle cannot be drawn as a single arc, so it is approximated.
            double sweep = Math.Min(this.Score, 99.9) / 100.0 * 360.0;
            double angle = (sweep - 90.0) * Math.PI / 180.0;

            Point start = new Point(CentreX, CentreY - Radius);
            Point end = new Point(
                CentreX + (Radius * Math.Cos(angle)),
                CentreY + (Radius * Math.Sin(angle)));

            ArcSegment arc = new ArcSegment
            {
                Point = end,
                Size = new Size(Radius, Radius),
                IsLargeArc = sweep > 180.0,
                SweepDirection = SweepDirection.Clockwise,
            };

            PathFigure figure = new PathFigure { StartPoint = start };
            figure.Segments.Add(arc);

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            this.ProgressArc.Data = geometry;
        }
    }
}