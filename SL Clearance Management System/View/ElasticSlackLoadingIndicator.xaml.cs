using System.Windows.Media.Animation;

namespace SLCMS.View {
    /// <summary>
    /// Interaction logic for ElasticSlackLoadingIndicator.xaml
    /// </summary>
    public partial class ElasticSlackLoadingIndicator {
        public ElasticSlackLoadingIndicator() {
            InitializeComponent();
            var lineAnimationStoryboard = FindResource("LineAnimationStoryboard") as Storyboard;
            IsVisibleChanged += delegate {
                if(IsVisible) {
                    lineAnimationStoryboard?.Begin();
                } else {
                    lineAnimationStoryboard?.Stop();
                }
            };
        }
    }
}
