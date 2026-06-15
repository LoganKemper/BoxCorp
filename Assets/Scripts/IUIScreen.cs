namespace BoxCorp
{
    public interface IUIScreen
    {
        bool IsVisible { get; }
        void Init();
        void Show();
        void Hide();
    }
}
