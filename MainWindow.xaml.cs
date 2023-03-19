using CodeBoxControl.Decorations;
using DiffDemo.JSON;
using DiffDemo.LCS;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;


namespace DiffDemo
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        static readonly Brush BRUSH_NOCHANGES = new SolidColorBrush(Color.FromRgb(0xA3, 0xA3, 0xA3));
        static readonly Brush BRUSH_REMOVED_VALUE = new SolidColorBrush(Color.FromRgb(0xFF, 0xA3, 0xA3));
        static readonly Brush BRUSH_NEW_VALUE = new SolidColorBrush(Color.FromRgb(0xDD, 0xFF, 0xDD));

        private DiffEntityTypes diffEntityType = DiffEntityTypes.Lines;
        

        public DiffEntityTypes DiffEntityType
        {
            get => diffEntityType;
            set
            {
                if (diffEntityType != value)
                {
                    diffEntityType = value;
                    NotifyPropertyChanged(nameof(DiffEntityType));
                }
            }
        }

        public string ValueBefore { get; set; } = App.ReadResource("SampleDataMock.json").ToString();
        public string ValueAfter { get; set; } = App.ReadResource("SampleDataMock.json").ToString();

        public ObservableCollection<string> CurrentDiff { get; set; } = new ObservableCollection<string>();

        private string jsonDiff = string.Empty;
        public string JsonDiff { get => jsonDiff; set { jsonDiff = value; NotifyPropertyChanged(nameof(JsonDiff)); } }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            PropertyChanged += (s, e) => { if (e.PropertyName == nameof(DiffEntityType)) DoDiff(); };
            tbLeft.TextChanged += (s, e) => DoDiff();
            tbRight.TextChanged += (s, e) => DoDiff();


        }

        private void DoDiff()
        {
            if (DiffEntityType == DiffEntityTypes.Chars)
            {
                var diffResult = Diff.RunLCS(ValueBefore.ToCharArray(), ValueAfter.ToCharArray());
                ColorDiffs(diffResult, (s) => 1);
            }
            else if (DiffEntityType == DiffEntityTypes.Words)
            {
                var diffResult = Diff.RunLCS(SplitWholeWords(ValueBefore), SplitWholeWords(ValueAfter));
                ColorDiffs(diffResult, (s) => s?.Length ?? 0);
            }
            else if (DiffEntityType == DiffEntityTypes.Lines)
            {
                var diffResult = Diff.RunLCS(SplitLines(ValueBefore), SplitLines(ValueAfter));
                ColorDiffs(diffResult, (s) => s?.Length ?? 0);
            }

            DiffJObject();
        }

        private void DiffJObject()
        {
            //JSON test
            try
            {
                var left = JsonUtils.GetTokens(ValueBefore).ToArray();
                var right = JsonUtils.GetTokens(ValueAfter).ToArray();

                Func<JToken, JToken, bool> cmp = (l, r) =>
                {
                    if (!Equals(l.Path, r.Path))
                        return false;

                    return Equals(l, r);
                };

                var diffJson = Diff.RunLCS(left, right, cmp);
                JsonDiff = string.Join('\n', diffJson.Where(x => !x.IsNone).Select(x=>$"{x} JPath='{x.LineValue?.Path}'"));
            }
            catch (Exception ex)
            {
                JsonDiff = ex.Message;
            }
        }


        private void ColorDiffs<T>(IEnumerable<LCS<T>.DiffResut<T>> diff, Func<T?, int> lengthOf) where T : IComparable<T>
        {
            CurrentDiff.Clear();
            tbLeft.Decorations.Clear();
            tbRight.Decorations.Clear();

            if (diff.Any(x => !x.IsNone))
            {
                int posL = 0, posR = 0;
                foreach (var chunk in diff)
                {
                    if (!chunk.IsNone)
                        CurrentDiff.Add(chunk.ToString());

                    var len = lengthOf(chunk.LineValue);

                    if (chunk.IsSubtract)
                        tbLeft.Decorations.Add(BackgroundDecorator(posL, len, BRUSH_REMOVED_VALUE));
                    else if (chunk.IsAdd)
                        tbRight.Decorations.Add(BackgroundDecorator(posR, len, BRUSH_NEW_VALUE));

                    if (!chunk.IsAdd)
                        posL += len;

                    if (!chunk.IsSubtract)
                        posR += len;
                }
            }

            tbLeft.InvalidateVisual();
            tbRight.InvalidateVisual();
        }

        #region Helpers


        private string[] SplitWholeWords(string str)
            => SplitCustomImpl(str,
                (c) => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') /*|| c == '\'' || c == ' '*/);

        private string[] SplitLines(string str)
            => SplitCustomImpl(str,
                (c) => (c != '\n' && c != '\r'));

        private string[] SplitCustomImpl(string str, Predicate<char> partOfEntity)
        {
            if (string.IsNullOrEmpty(str))
                return new string[0];

            var res = new List<string>();
            var sb = new StringBuilder();
            foreach (char c in str)
            {
                if (partOfEntity(c))
                    sb.Append(c);
                else
                {
                    if (sb.Length > 0)
                    {
                        res.Add(sb.ToString());
                        sb.Clear();
                    }
                    if (c == '\n' && res[^1] == "\r")
                        res[^1] += c;
                    else
                        res.Add(c.ToString());
                }
            }
            return res.ToArray();
        }
        private ExplicitDecoration BackgroundDecorator(int chunkStartIdx, int chunkLength, Brush brush) => new()
        {
            Start = chunkStartIdx,
            Length = chunkLength,
            Brush = brush,
            DecorationType = EDecorationType.Hilight
        };


        #endregion

        #region ---INotifyPropertyChanged---
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion


    }
}
