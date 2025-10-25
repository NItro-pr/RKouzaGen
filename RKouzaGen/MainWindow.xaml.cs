using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RKouzaGen
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void NormalizeButton_Click(object sender, RoutedEventArgs e)
		{
			NormalizedList_TextBox.Text = string.Join('\n', NameList_TextBox.Text.Replace("\r\n", "\n").Split(new[] { '\n', '\r' }).Select(x => x.Split('\t')[0]));
			AssignmentGenerateButton.IsEnabled = true;
		}

		private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
		{
			YearTextBox.Text = DateTime.Now.Year.ToString();
			MonthTextBox.Text = (DateTime.Now.Month + 1).ToString();
		}

		private void AssignmentGenerateButton_Click(object sender, RoutedEventArgs e)
		{
			Assignment_TextBox.Text = "";

			int num = 2;//例会ごとに行われる講座の数、最低でも2講座分の枠は用意する

			//例会が行われる曜日
			DayOfWeek[] reikaiDays = [DayOfWeek.Monday, DayOfWeek.Thursday];

			//例会が行われる日付
			var reikaiDates = new List<DateTime>();

			//その月の月曜日と木曜日を取得する
			var date = new DateTime(int.Parse(YearTextBox.Text), int.Parse(MonthTextBox.Text), 1);

			var month = date.Month;
			while (month == date.Month)
			{
				if (reikaiDays.Any(x => x == date.DayOfWeek))
				{
					reikaiDates.Add(date);
				}
				date = date.AddDays(1);
			}

			//会員名簿
			var names = NormalizedList_TextBox.Text.Split("\n").Where(x => x != "" && x != " ").OrderBy(a => Guid.NewGuid()).ToList();

			//講座の枠に対して人数が多すぎた場合
			if (names.Count > reikaiDates.Count * num)
			{
				num = Convert.ToInt32(names.Count / reikaiDates.Count) + 1;
			}
			//逆に少なすぎたら誰かに2回以上やってもらう
			else if (reikaiDates.Count > names.Count)
			{
				var extra = new List<string>();
				for (int i = 0; i < ((reikaiDates.Count - names.Count) / names.Count); i++)
				{
					extra.AddRange(names.OrderBy(a => Guid.NewGuid()));
				}

				//あまり
				int a = (reikaiDates.Count) % names.Count;
				if (a > 0)
				{
					var l = names.OrderBy(a => Guid.NewGuid()).ToList();
					extra.AddRange(Enumerable.Range(0, a).Select(x => l[x]));
				}

				names.AddRange(extra);
			}

			string header = $"**{MonthTextBox.Text}月の例会の部屋と講座担当者\n" +
							$"|日付|場所|{string.Join("|", Enumerable.Range(0, num).Select(x => $">|>|講座{x + 1}"))}|例会の議題|備考(プロジェクタ不要とか)|h\n" +
							$"|~|~|{string.Join("|", Enumerable.Range(0, num).Select(x => "担当|内容|概要"))}|~|~|h\n";

			int quotient = Math.DivRem(names.Count, reikaiDates.Count, out int remainder);

			var wikiwrite = names.Select(x => $"{x}|||").ToList();

			//ぴったり割り当てられずに余りができた場合(縦の8枠に4人しか入らないとか)
			if (names.Count % reikaiDates.Count != 0)
			{
				wikiwrite.AddRange(Enumerable.Range(0, reikaiDates.Count - remainder).Select(x => "|||"));
			}

			//1列目しか埋まらなかったら空の列を追加する
			if (names.Count == reikaiDates.Count)
			{
				wikiwrite.AddRange(Enumerable.Range(0, reikaiDates.Count).Select(x => "|||"));
			}

			List<string> reses = new List<string>();

			//行のはじめ(日付、場所)
			for (int i = 0; i < reikaiDates.Count; i++)
			{
				reses.Add($"|{reikaiDates[i].ToString("MM/dd(ddd)")}|部室\\Discord|");
			}

			//名簿
			for (int k = 0; k < num; k++)
			{
				for (int i = 0; i < reikaiDates.Count; i++)
				{
					reses[i] += wikiwrite[reikaiDates.Count * k + i];
				}
			}

			//フッター
			for (int i = 0; i < reikaiDates.Count; i++)
			{
				reses[i] += "||";
			}

			//テキストボックスに表示
			Assignment_TextBox.Text += header;
			Assignment_TextBox.Text += string.Join("\n", reses);
		}

		private async void CopyToClipboardButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Clipboard.Clear();
				await Task.Delay(100);
				Clipboard.SetText(Assignment_TextBox.Text);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"クリップボードへの貼り付けに失敗しました。{ex.Message}");
			}
		}
	}
}