using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace taskConnectProject.ViewModel
{
    public class HelpViewModel : BindableObject
    {
        public ObservableCollection<FAQItem> FAQs { get; set; }
        public ICommand ToggleFAQCommand { get; }

        public HelpViewModel()
        {
            FAQs = new ObservableCollection<FAQItem>
            {
                new FAQItem("How do I create a task?", "To create a task, go to the home screen, click on 'Post a Task' and fill in the details."),
                new FAQItem("How do I find taskers?", "Search for taskers based on skills and availability in the 'Find Taskers' section."),
                new FAQItem("Is payment secure?", "Yes, all transactions are securely processed through our integrated payment gateway."),
                new FAQItem("Can I cancel a task?", "Yes, tasks can be canceled before they are accepted by a tasker."),
            };
            ToggleFAQCommand = new Command<FAQItem>(ToggleFAQ);
        }

        private void ToggleFAQ(FAQItem faq)
        {
            faq.IsExpanded = !faq.IsExpanded;
            OnPropertyChanged(nameof(FAQs));
        }
    }

    public class FAQItem : BindableObject
    {
        public string Question { get; set; }
        public string Answer { get; set; }
        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public FAQItem(string question, string answer)
        {
            Question = question;
            Answer = answer;
            IsExpanded = false;
        }
    }
}
