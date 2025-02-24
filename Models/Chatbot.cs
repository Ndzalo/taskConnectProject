namespace taskConnectProject.Models;

public class MsgTempSelector : DataTemplateSelector
{
    public DataTemplate userTemp {  get; set; }
    public DataTemplate botTemp { get; set; }
    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is ChatMessages chatMessages)
        {
            return chatMessages.IsUserMessage ? userTemp : botTemp; 
        }

        return null; 
    }
}
