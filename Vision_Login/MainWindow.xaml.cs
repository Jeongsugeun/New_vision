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

namespace Vision_Login
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // 시작 시 Operator가 기본 선택이므로 Password창 숨기고 Placeholder 초기화
            gridPassword.Visibility = Visibility.Hidden;
            txtPasswordPlaceholder.Visibility = Visibility.Visible;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // ComboBox 선택 변경 시: Operator면 비밀번호 숨기기
        private void cmbUserLevel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 초기화 중 gridPassword가 아직 null일 수 있으므로 체크
            if (gridPassword == null || txtPassword == null) return;

            if (cmbUserLevel.SelectedItem is ComboBoxItem selected)
            {
                string level = selected.Content?.ToString() ?? "";

                if (level == "Operator")
                {
                    gridPassword.Visibility = Visibility.Hidden;
                }
                else
                {
                    gridPassword.Visibility = Visibility.Visible;
                    txtPassword.Clear();
                    txtPasswordPlaceholder.Visibility = Visibility.Visible;
                }
            }
        }

        // 비밀번호 입력 시 Placeholder 숨기기
        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtPasswordPlaceholder == null) return;
            txtPasswordPlaceholder.Visibility = txtPassword.Password.Length > 0
                ? Visibility.Hidden : Visibility.Visible;
        }

        // 비밀번호창 클릭(포커스) 시 Placeholder 숨기기
        private void txtPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtPasswordPlaceholder == null) return;
            txtPasswordPlaceholder.Visibility = Visibility.Hidden;
        }

        // 비밀번호창 포커스 잃을 때 비어있으면 Placeholder 다시 보이기
        private void txtPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtPasswordPlaceholder == null) return;
            txtPasswordPlaceholder.Visibility = txtPassword.Password.Length == 0
                ? Visibility.Visible : Visibility.Hidden;
        }

        // Login Now 버튼 클릭 시 비밀번호 검증
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (cmbUserLevel.SelectedItem is ComboBoxItem selected)
            {
                string level = selected.Content?.ToString() ?? "";
                bool loginSuccess = false;

                if (level == "Operator")
                {
                    loginSuccess = true;
                }
                else if (level == "Engineer")
                {
                    if (txtPassword.Password == "9999")
                        loginSuccess = true;
                    else
                        MessageBox.Show("패스워드가 틀렸습니다. 다시 입력해주세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else if (level == "Admin")
                {
                    if (txtPassword.Password == "0000")
                        loginSuccess = true;
                    else
                        MessageBox.Show("패스워드가 틀렸습니다. 다시 입력해주세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                if (loginSuccess)
                {
                    Cam main = new Cam();
                    main.Show();
                    this.Close();
                }
            }
        }

        // 타이틀 바가 없으므로 창을 마우스로 끌 수 있게 해주는 코드
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.DragMove();
        }
    }
}
