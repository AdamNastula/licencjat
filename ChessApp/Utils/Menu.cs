namespace ChessApp.Utils;

public class Menu : VerticalStackLayout
{
    private HistoryElement _movesHistory;
    public Menu()
    {
        WidthRequest = 8 * GameBoard.SquareSize;
        HorizontalOptions = this.HorizontalOptions with { Alignment = LayoutAlignment.End };
        
        Add(new Label()
        {
            Text = "Historia ruchów",
            FontAttributes = FontAttributes.Bold,
            FontSize = 30,
            WidthRequest = this.Width,
            HorizontalTextAlignment = TextAlignment.Center,
        });
        
        _movesHistory = new HistoryElement();
        _movesHistory.HorizontalOptions = _movesHistory.HorizontalOptions with { Alignment = LayoutAlignment.Center};
        Add(_movesHistory);
        
        Label timeLabel = new Label
        {
            FontSize = 30,
            Text = "0:00"
        };
        timeLabel.HorizontalOptions = timeLabel.HorizontalOptions with { Alignment = LayoutAlignment.Center };
        Add(timeLabel);
        
        Add(new Button()
        {
            Text = "Zaproponuj remis",
            WidthRequest = 200,
            Margin = new Thickness(0, 10)
        });
        
        Add(new Button
        {
            Text = "Poddaj się",
            WidthRequest = 200,
            Margin = new Thickness(0, 10)
        });
    }
}