import requests
from PyQt6.QtGui import QFont
from PyQt6.QtWidgets import (
    QWidget, QLabel, QPushButton,
    QVBoxLayout, QMessageBox, QHBoxLayout, QSpinBox, QSlider, QInputDialog
)
from PyQt6.QtCore import Qt
from Settings import API_KEY


Wallet: {str, int} = {}

# Map from ID to a wallet key (adjust as needed)
ID_TO_COIN = {
    90: "bitcoin",
    80: "ethereum",
    70: "litecoin",
    60: "ripple"
}


class BuySellWindow(QWidget):
    def __init__(self, action, currency_data, currency_id):
        super().__init__()
        self.wallet = Wallet
        self.API_KEY = API_KEY
        self.ID_TO_COIN = ID_TO_COIN
        self.currency_id = currency_id

        self.setWindowTitle(f"{action} {currency_data['name']}")
        self.setFixedSize(420, 400)
        self.currency_data = currency_data
        self.action = action.lower()

        self.add_money_button = QPushButton("Add Money")
        self.back_button = QPushButton("Back")
        self.label = QLabel(f"{self.action.capitalize()} - Price: ${self.currency_data['price_usd']}")
        self.amount_input = QSpinBox()
        self.total_price_label = QLabel("Total Price: $0.00")

        self.init_ui()
        self.refresh_wallet_display()

    def init_ui(self):
        layout = QVBoxLayout()
        layout.setContentsMargins(20, 20, 20, 20)
        layout.setSpacing(12)

        self.setStyleSheet("""
            QWidget {
                background-color: #1e1f26;
                color: white;
            }
        """)

        button_style = "background-color: #FFD700; color: black; font-weight: bold; padding: 8px;"

        top_layout = QHBoxLayout()

        self.back_button.setStyleSheet("background-color: gray; color: #FFD700; font-size: 10px;")
        self.back_button.clicked.connect(self.close)
        top_layout.addWidget(self.back_button, alignment=Qt.AlignmentFlag.AlignLeft)
        top_layout.addStretch()
        layout.addLayout(top_layout)



        self.label.setFont(QFont("Arial", 14))
        layout.addWidget(self.label)

        self.amount_input.setRange(1, 100000)
        self.amount_input.setPrefix("Amount: ")
        self.amount_input.setFont(QFont("Arial", 12))
        self.amount_input.valueChanged.connect(self.update_total_price)
        layout.addWidget(self.amount_input)

        layout.addWidget(self.total_price_label)

        button_layout = QHBoxLayout()
        self.total_price_label = QLabel("Total Price: $0.00")
        self.confirm_button.setStyleSheet(button_style)
        self.confirm_button.clicked.connect(self.confirm_transaction)
        button_layout.addWidget(self.confirm_button)


        self.add_money_button.setStyleSheet(button_style)
        self.add_money_button.clicked.connect(self.add_money)
        button_layout.addWidget(self.add_money_button)

        layout.addLayout(button_layout)

        self.wallet_status_label = QLabel(" ")
        self.wallet_status_label.setFont(QFont("Arial", 12))
        layout.addWidget(self.wallet_status_label)

        self.balance_button = QPushButton("Check Wallet Balance")
        self.balance_button.setStyleSheet(button_style)
        self.balance_button.clicked.connect(self.refresh_wallet_display)
        layout.addWidget(self.balance_button)

        # self.back_button = QPushButton("Back")
        # self.back_button.setStyleSheet(button_style)
        # self.back_button.clicked.connect(self.close)
        # layout.addWidget(self.back_button)
        self.setLayout(layout)

    def update_amount_from_slider(self, percent):
        if self.action == "buy":
            dollar_balance = self.wallet.get("usd_balance", 0)
            amount_dollars = (percent / 100) * dollar_balance
            units = amount_dollars / float(self.currency_data['price_usd'])
        else:  # sell
            coin_balance = self.wallet.get(self.currency_data['id'], 0)
            units = (percent / 100) * coin_balance

        self.amount_input.setValue(int(units))
        self.update_total_price()

    def update_wallet_local(self, currency_id: int, amount: float, action: str):
        coin_key = self.ID_TO_COIN.get(currency_id, "otherCrypto")
        price_per_unit = float(self.currency_data["price_usd"])
        usd_change = amount * price_per_unit

        if action == "buy":
            self.wallet["balance"] -= usd_change
            self.wallet[coin_key] = self.wallet.get(coin_key, 0) + amount
        elif action == "sell":
            self.wallet["balance"] += usd_change
            self.wallet[coin_key] = self.wallet.get(coin_key, 0) - amount

    def update_total_price(self):
        units = self.amount_input.value()
        price_per_unit = float(self.currency_data['price_usd'])
        total = units * price_per_unit
        self.total_price_label.setText(f"Total Price: ${total:.2f}")

    def refresh_wallet_display(self):
        try:
            url = "http://localhost:5182/api/APIServices/WalletBalance"
            response = requests.get(url, headers={"X-Api-Key": self.API_KEY})
            if response.status_code == 200:
                self.wallet.update(response.json().get('walletBalance'))
                usd_balance = self.wallet.get("balance", 0)
                coin_balance = self.wallet.get(self.ID_TO_COIN.get(self.currency_id, None), 0)
                self.wallet_status_label.setText(
                    f"Wallet: ${usd_balance:.2f}\n\n{self.currency_data['name']}: {coin_balance:.2f}"
                )
            else:
                QMessageBox.warning(self, "Error", "Failed to retrieve wallet.")
        except Exception as e:
            QMessageBox.critical(self, "Error", str(e))

    def add_money(self):
        amount, ok = QInputDialog.getInt(self, "Add Money", "Enter amount to add:", min=1)
        if ok:
            try:
                url = "http://localhost:5182/api/APIServices/AddMoney"
                data = {"amount": amount}
                response = requests.post(url, headers={"X-Api-Key": self.API_KEY}, data=data)
                if response.status_code == 200:
                    QMessageBox.information(self, "Success", f"${amount} added to your wallet.")
                    self.refresh_wallet_display()
                else:
                    QMessageBox.warning(self, "Error", "Failed to add money.")
            except Exception as e:
                QMessageBox.critical(self, "Error", f"An error occurred:\n{str(e)}")

    def confirm_transaction(self):
        amount = self.amount_input.value()
        try:
            url = f"http://localhost:5182/api/APIServices/{self.action}"
            data = {
                "id": self.currency_data["id"],
                "amount": amount
            }
            response = requests.post(url, headers={"X-Api-Key": self.API_KEY}, data=data)
            if response.status_code == 200:
                QMessageBox.information(self, "Success", f"{self.action.capitalize()} completed successfully.")
                self.refresh_wallet_display()
                self.update_wallet_local(self.currency_data["id"], amount, self.action)
                self.close()
            else:
                QMessageBox.warning(self, "Failed", f"{self.action.capitalize()} failed.")
        except Exception as e:
            QMessageBox.critical(self, "Error", f"An error occurred:\n{str(e)}")
