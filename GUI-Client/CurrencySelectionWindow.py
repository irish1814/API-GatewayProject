import requests
from PyQt6.QtWidgets import QWidget, QVBoxLayout, QLabel, QListWidget, QPushButton, QHBoxLayout
from PyQt6.QtCore import Qt
from CurrencyDataWindow import CurrencyListWindow
from Settings import API_KEY, API_SERVER


class CurrencySelectionWindow(QWidget):
    def __init__(self):
        super().__init__()
        self.setWindowTitle("Choose Crypto currency")
        self.setFixedSize(400, 550)
        self.API_KEY = API_KEY
        response = requests.get(API_SERVER + "APIServices/SupportedCurrencies", headers={"X-Api-Key": self.API_KEY})
        if response.status_code == 200:
            self.ID_MAP = response.json().get("supportedCurrencies")

        self.setStyleSheet("""
            QWidget {
                background-color: #1e1f26;  /* כחול כהה מודרני */
                color: #f0f0f0;
            }

            QLabel {
                font-size: 20px;
                font-weight: bold;
                padding: 10px;
            }

            QListWidget {
                background-color: #2c2f38;
                border: none;
                border-radius: 10px;
                padding: 5px;
                font-size: 16px;
            }

            QListWidget::item {
                padding: 10px;
                color: #ffffff;
            }

            QListWidget::item:hover {
                background-color: #3d414e;
                color: #ffd700;  /* זהב */
            }

            QListWidget::item:selected {
                background-color: #5a5f73;
                color: #ffd700;
            }

            QPushButton {
                background-color: transparent;
                color: #bbbbbb;
                font-size: 14px;
                border: none;
                padding: 5px;
            }

            QPushButton:hover {
                color: #ffffff;
                text-decoration: underline;
            }
        """)

        main_layout = QVBoxLayout()
        main_layout.setSpacing(15)
        main_layout.setContentsMargins(20, 20, 20, 20)

        # top_layout = QHBoxLayout()
        # self.signout_button = QPushButton("Sign Out")
        # self.signout_button.clicked.connect(self.signout)
        # top_layout.addWidget(self.signout_button)
        # top_layout.addStretch()
        # main_layout.addLayout(top_layout)

        label = QLabel("Choose Crypto Currency")
        label.setAlignment(Qt.AlignmentFlag.AlignCenter)
        main_layout.addWidget(label)

        self.crypto_list = QListWidget()
        self.crypto_list.addItems([name for name in self.ID_MAP.keys()])
        self.crypto_list.itemDoubleClicked.connect(self.open_currency_detail)
        main_layout.addWidget(self.crypto_list)

        self.setLayout(main_layout)

    def open_currency_detail(self, item):
        name = item.text()

        crypto_id = self.ID_MAP.get(name, 0)
        self.detail_window = CurrencyListWindow(name, crypto_id)
        self.detail_window.show()
        self.close()

    # def signout(self):
    #     self.API_KEY = ""
    #     self.login_window.show()
    #     self.close()
