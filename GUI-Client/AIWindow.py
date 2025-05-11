import requests
from PyQt6.QtWidgets import (
    QWidget, QPushButton,
    QVBoxLayout, QHBoxLayout, QTextEdit, QLineEdit
)
from Settings import API_KEY

class AIChatWindow(QWidget):
    def __init__(self):
        super().__init__()
        self.API_KEY = API_KEY
        self.setWindowTitle("AI Agent")
        self.setFixedSize(400, 500)
        self.setStyleSheet("background-color: #1e1e1e; color: white;")

        layout = QVBoxLayout()

        self.chat_history = QTextEdit()
        self.chat_history.setReadOnly(True)
        self.chat_history.setStyleSheet("background-color: #2e2e2e; color: white;")
        layout.addWidget(self.chat_history)

        input_layout = QHBoxLayout()
        self.user_input = QLineEdit()
        self.user_input.setPlaceholderText("Ask me a question...")
        self.user_input.setStyleSheet("background-color: #333; color: white; padding: 5px;")
        input_layout.addWidget(self.user_input)

        send_button = QPushButton("Send")
        send_button.setStyleSheet("background-color: #FFD700; color: black; font-weight: bold;")
        send_button.clicked.connect(self.handle_user_input)
        input_layout.addWidget(send_button)

        layout.addLayout(input_layout)
        self.setLayout(layout)

    def handle_user_input(self):
        user_text = self.user_input.text().strip()
        if not user_text:
            return

        self.chat_history.append(f"<b>You:</b> {user_text}")
        self.user_input.clear()

        # זמני: תשובה מדומה, אפשר לשלב כאן קריאה ל־API
        url = "http://localhost:5182/api/APIServices/Agent"
        header = {"X-Api-key": self.API_KEY}
        data = {"prompt": user_text}
        response = requests.post(url, data=data, headers=header)
        if response.status_code == 200:
            bot_reply = response.json()['agentResponse']
            self.chat_history.append(f"<b>AI Agent:</b> {bot_reply}")

