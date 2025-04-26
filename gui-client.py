import sys
import requests
from PyQt6.QtWidgets import QApplication, QWidget, QPushButton, QVBoxLayout, QTextEdit

class CryptoFetcher(QWidget):
    def __init__(self):
        super().__init__()
        self.init_ui()

    def init_ui(self):
        self.setWindowTitle("CryptoCurrency Fetcher")
        self.setGeometry(100, 100, 400, 300)

        # Create a layout
        layout = QVBoxLayout()

        # Create a button
        self.button = QPushButton("Fetch Crypto Data")
        self.button.clicked.connect(self.fetch_data)

        # Create a text area to show the result
        self.text_area = QTextEdit()
        self.text_area.setReadOnly(True)

        # Add widgets to the layout
        layout.addWidget(self.button)
        layout.addWidget(self.text_area)

        # Set the layout for the window
        self.setLayout(layout)

    def fetch_data(self):
        url = "http://localhost:5182/api/APIServices/cryptoCurrency"
        try:
            response = requests.get(url)
            response.raise_for_status()
            data = response.json()
            self.text_area.setText(str(data))
        except requests.exceptions.RequestException as e:
            self.text_area.setText(f"An error occurred: {e}")

if __name__ == "__main__":
    app = QApplication(sys.argv)
    window = CryptoFetcher()
    window.show()
    sys.exit(app.exec())
