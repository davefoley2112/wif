namespace WPCloudApp.Phone.Pages
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Navigation;
    using Microsoft.Phone.Controls;
    using WPCloudApp.Phone.Helpers;
    using WPCloudApp.Phone.Models;
    using WPCloudApp.Phone.ViewModel;

    public partial class SampleDataDetailsPage : PhoneApplicationPage
    {
        public SampleDataDetailsPage()
        {
            this.InitializeComponent();

            var viewModel = new SampleDataDetailsPageViewModel();
            viewModel.SaveChangesSuccess += (s, e) => this.NavigationService.GoBack();

            this.DataContext = viewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var viewModel = this.DataContext as SampleDataDetailsPageViewModel;
            if (viewModel != null)
            {
                var editSampleDataAsString = this.NavigationContext.QueryString["editSampleData"];
                bool editSampleData;

                if (string.IsNullOrWhiteSpace(editSampleDataAsString) || !bool.TryParse(editSampleDataAsString, out editSampleData))
                {
                    editSampleData = false;
                }

                if (viewModel.SampleData == null)
                {
                    var sampleData = PhoneHelpers.GetApplicationState<SampleData>("CurrentSampleDataRow");
                    if (editSampleData && (sampleData == null))
                    {
                        this.NavigationService.GoBack();
                    }

                    viewModel.SetSampleDataModel(sampleData);
                    PhoneHelpers.RemoveApplicationState("CurrentSampleDataRow");
                }
            }
        }

        private void OnTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == this.PartitionKeyTextBox)
                {
                    this.RowKeyTextBox.Focus();
                }
                else if (sender == this.RowKeyTextBox)
                {
                    this.NameTextBox.Focus();
                }
                else if (sender == this.NameTextBox)
                {
                    this.DescriptionTextBox.Focus();
                }
                else if (sender == this.DescriptionTextBox)
                {
                    this.NumberTextBox.Focus();
                }
                else if (sender == this.NumberTextBox)
                {
                    this.DateDatePicker.Focus();
                }
                else
                {
                    this.PartitionKeyTextBox.Focus();
                }
            }
        }

        private void OnDeleteSampleData(object sender, EventArgs e)
        {
            var viewModel = this.DataContext as SampleDataDetailsPageViewModel;
            if (viewModel != null)
            {
                viewModel.DeleteSampleData();
            }
        }

        private void OnSaveSampleData(object sender, EventArgs e)
        {
            var viewModel = this.DataContext as SampleDataDetailsPageViewModel;
            if (viewModel != null)
            {
                var focusTextbox = FocusManager.GetFocusedElement() as TextBox;
                if (focusTextbox != null)
                {
                    var binding = focusTextbox.GetBindingExpression(TextBox.TextProperty);
                    binding.UpdateSource();
                }

                viewModel.SaveSampleData();
            }
        }
    }
}