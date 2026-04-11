using ADHDCompanionApp.Models.Entities;

namespace ADHDCompanionApp.Helpers;

public class ChatMessageTemplateSelector : DataTemplateSelector
{
    public DataTemplate ArloTemplate { get; set; }
    public DataTemplate UserTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        if (item is ChatMessage message)
        {
            return message.IsFromUser ? UserTemplate : ArloTemplate;
        }

        return ArloTemplate;
    }
}