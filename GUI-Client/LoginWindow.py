from CurrencySelectionWindow import CurrencySelectionWindow
from PyQt6.QtWidgets import (
    QApplication, QLineEdit,
    QMessageBox, QCheckBox,
)
from PyQt6.QtGui import QMovie
from PyQt6.QtWidgets import QWidget, QVBoxLayout, QLabel, QPushButton
from PyQt6.QtGui import QFont
from PyQt6.QtCore import Qt
import requests
from SignupWindow import RegisterWindow
from Settings import API_SERVER
import sys


class LoginWindow(QWidget):
    def __init__(self):
        super().__init__()
        self.register_window = RegisterWindow()
        self.form_container = QWidget(self)
        self.username_input = QLineEdit()
        self.password_input = QLineEdit()
        self.show_password_checkbox = QCheckBox("Show password")
        self.login_button = QPushButton("Login")
        self.currency_window = CurrencySelectionWindow()

        self.setWindowTitle("Welcome to trading system")
        self.setFixedSize(400, 550)

        self.movie = QMovie("assets/background.gif")
        self.movie.start()

        self.background_label = QLabel(self)
        self.background_label.setMovie(self.movie)
        self.background_label.setGeometry(0, 0, 400, 555)
        self.background_label.setScaledContents(True)
        self.background_label.lower()

        self.apply_styles()
        self.setup_ui()

    def apply_styles(self):
        self.form_container.setStyleSheet("""
            background-color: rgba(0, 0, 0, 150);  /* semi-transparent dark background */
            border-radius: 15px;
        """)

        self.username_input.setStyleSheet("""
            QLineEdit {
                background-color: #ffffff;
                border: 1px solid #cccccc;
                border-radius: 8px;
                padding: 8px;
            }
            QLineEdit:focus {
                border: 2px solid #0078d7;
            }
        """)

        self.password_input.setStyleSheet(self.username_input.styleSheet())

        self.login_button.setStyleSheet("""
            QPushButton {
                background-color: #0078d7;
                color: white;
                font-size: 14px;
                border-radius: 8px;
                padding: 10px;
            }
            QPushButton:hover {
                background-color: #005a9e;
            }
            QPushButton:pressed {
                background-color: #003f6f;
            }
        """)

        self.show_password_checkbox.setStyleSheet("""
            QCheckBox {
                background-color: rgba(0, 0, 0, 0);
                color: white;
                font-size: 12px;
            }
        """)

        self.setStyleSheet("""
            QLabel {
                color: white;
                font-size: 13px;
            }
            QLabel:hover a{
                color: #00aaff;
            }
            QLabel::link {
                color: #00aaff;
                text-decoration: none;
            }
            QLabel::link:hover {
                text-decoration: underline;
            }
        """)

    def setup_ui(self):
        self.form_container.setFixedSize(300, 350)
        self.form_container.move(
            (self.width() - self.form_container.width()) // 2,
            (self.height() - self.form_container.height()) // 2
        )

        layout = QVBoxLayout(self.form_container)
        layout.setContentsMargins(20, 20, 20, 20)
        layout.setSpacing(7)

        self.username_input.setPlaceholderText("Email")
        self.username_input.setFont(QFont("Arial", 16))
        layout.addWidget(self.username_input)

        self.password_input.setPlaceholderText("Password")
        self.password_input.setEchoMode(QLineEdit.EchoMode.Password)
        self.password_input.setFont(QFont("Arial", 16))
        layout.addWidget(self.password_input)

        self.show_password_checkbox.stateChanged.connect(self.toggle_password_visibility)
        layout.addWidget(self.show_password_checkbox)

        self.login_button.clicked.connect(self.handle_login)
        layout.addWidget(self.login_button)

        register_label = QLabel("<p>Don't have an account? <a href='#'>Signup here""</a></p>")
        register_label.setAlignment(Qt.AlignmentFlag.AlignLeft)

        register_label.linkActivated.connect(self.open_register_window)
        layout.addWidget(register_label)

    def toggle_password_visibility(self, state):
        self.password_input.setEchoMode(
            QLineEdit.EchoMode.Normal if state == Qt.CheckState.Checked.value else QLineEdit.EchoMode.Password
        )

    def handle_login(self):
        email = self.username_input.text()
        password = self.password_input.text()

        url = API_SERVER + "Users/login"
        data = {"email": email, "password": password}
        try:
            response = requests.post(url, data=data)
            if response.status_code == 200:
                with open(".env", "w") as f:
                    f.write(f"API_KEY={response.json()['apiKey']}")

                self.currency_window.show()
                self.close()
            else:
                QMessageBox.warning(self, "ERROR", "Wrong username or password.")

        except requests.exceptions.RequestException as e:
            QMessageBox.critical(self, "ERROR", f"Network error: {e}")

    def open_register_window(self):
        self.register_window.show()


if __name__ == "__main__":
    app = QApplication(sys.argv)
    window = LoginWindow()
    window.show()
    sys.exit(app.exec())
