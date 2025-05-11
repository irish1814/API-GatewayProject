import requests
from PyQt6.QtWidgets import (
    QWidget, QLineEdit, QPushButton,
    QVBoxLayout, QMessageBox, QLabel
)
from Settings import API_SERVER


class RegisterWindow(QWidget):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Sign up")
        self.setFixedSize(400, 500)
        self.setStyleSheet("background-color: rgba(0, 0, 0, 150); color: #FFD700;")

        layout = QVBoxLayout()
        layout.setContentsMargins(20, 20, 20, 20)
        layout.setSpacing(10)
        title = QLabel("Create a New Account")
        title.setStyleSheet("font-size: 18px; font-weight: bold; color: white;")
        layout.addWidget(title)
        self.username_input = QLineEdit("")
        self.username_input.setPlaceholderText("Username")
        self.email_input = QLineEdit("")
        self.email_input.setPlaceholderText("Email")
        self.password_input = QLineEdit("")
        self.password_input.setPlaceholderText("Password")
        self.password_input.setEchoMode(QLineEdit.EchoMode.Password)
        self.confirm_input = QLineEdit("")
        self.confirm_input.setPlaceholderText("Verify password")
        self.confirm_input.setEchoMode(QLineEdit.EchoMode.Password)

        self.register_button = QPushButton("Sign up")
        self.register_button.clicked.connect(self.register_user)
        self.back_button = QPushButton("back")
        self.back_button.clicked.connect(self.close)

        for widget in [self.username_input, self.email_input, self.password_input,
                    self.confirm_input, self.register_button, self.back_button]:
            layout.addWidget(widget)

        self.apply_styles()
        self.setLayout(layout)

    def apply_styles(self):
        self.setStyleSheet("""
            QWidget {
                background-color: rgba(0, 0, 0, 180);
            }
            QLineEdit {
                background-color: #ffffff;
                color: black;
                border: 1px solid #cccccc;
                border-radius: 8px;
                padding: 8px;
                font-size: 14px;
            }
            QLineEdit:focus {
                border: 2px solid #0078d7;
            }
            QPushButton {
                background-color: #0078d7;
                color: white;
                font-weight: bold;
                border-radius: 8px;
                padding: 10px;
                font-size: 14px;
            }
            QPushButton:hover {
                background-color: #005a9e;
            }
            QPushButton:pressed {
                background-color: #003f6f;
            }
        """)

    def register_user(self):
        if self.password_input.text() != self.confirm_input.text():
            QMessageBox.warning(self, "ERROR", "Passwords don't match")
        else:
            email = self.email_input.text()
            password = self.password_input.text()
            username = self.username_input.text()
            url = API_SERVER + "Users/register"
            data = {"email": email,"username": username , "password": password}
            try:
                response = requests.put(url, data=data)
                if response.status_code == 200:
                    QMessageBox.information(self, "Signup", "Successfully Signup")
                else:
                    QMessageBox.warning(self, "ERROR", "Wrong username or password")
            except requests.exceptions.RequestException as e:
                QMessageBox.critical(self, "ERROR", f"Network error: {e}")

            self.close()
