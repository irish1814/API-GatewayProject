from datetime import datetime
import requests
import traceback

from PyQt6.QtGui import QFont
from PyQt6.QtWidgets import (
    QWidget, QLabel, QPushButton,
    QVBoxLayout, QMessageBox, QHBoxLayout, QFrame
)
import json
import pyqtgraph as pg
from PyQt6.QtCore import Qt

from AIWindow import AIChatWindow
from Transactions import BuySellWindow
from Settings import API_KEY


class CurrencyListWindow(QWidget):
    def __init__(self, currency_name, currency_id):
        super().__init__()
        self.API_KEY = API_KEY
        self.setWindowTitle(currency_name)
        self.setFixedSize(1100, 900)
        self.currency_id = currency_id

        self.layout = QVBoxLayout()
        self.setStyleSheet("background-color: #1e1f26; color: #FFD700;")

        self.back_button = QPushButton("Back")
        # self.back_button.clicked.connect(self.go_back)
        self.back_button.setStyleSheet("background-color: gray; font-size: 10px;")
        self.layout.addWidget(self.back_button, alignment=Qt.AlignmentFlag.AlignLeft)

        self.info_label = QLabel("Loading...")
        self.info_label.setStyleSheet("font-size: 14px; padding: 4px;")
        self.layout.addWidget(self.info_label)

        # Chart Area
        chart_frame = QFrame()
        chart_layout = QVBoxLayout()

        self.plot_widget = pg.PlotWidget()
        self.plot_widget.setBackground('w')
        self.plot_widget.showGrid(x=True, y=True)
        self.plot_data = self.plot_widget.plot([], pen=pg.mkPen(color='b', width=2))
        chart_layout.addWidget(self.plot_widget)
        chart_frame.setLayout(chart_layout)

        self.layout.addWidget(chart_frame, 2)

        self.image_label = QLabel("")
        self.layout.addWidget(self.image_label)

        btn_layout = QHBoxLayout()
        self.buy_button = QPushButton("Buy")
        self.buy_button.clicked.connect(self.open_buy)
        self.sell_button = QPushButton("Sell")
        self.sell_button.clicked.connect(self.open_sell)
        self.ai_button = QPushButton("AI Agent")
        self.ai_button.clicked.connect(self.open_ai_chat)


        for btn in [self.buy_button, self.sell_button, self.ai_button]:
            btn.setStyleSheet("background-color: #FFD700; color: black; font-weight: bold; padding: 8px;")
            btn_layout.addWidget(btn)

        self.layout.addLayout(btn_layout)
        self.setLayout(self.layout)

        self.currency_data = self.load_currency_data()

    def load_currency_data(self):
        try:
            response = requests.post("http://localhost:5182/api/APIServices/CurrencyInfo", 
                                    data={"id": self.currency_id}, headers={"X-Api-Key": self.API_KEY})
            if response.status_code == 200:
                data = json.loads(response.json()['currencyData'])[0]
                self.info_label.setText(f"Rank: {data['rank']} | Current price: {data['price_usd']}$")

                # גרף
                if response.json().get("currencyHistory", None):
                    history_list = response.json().get("currencyHistory")
                    self.draw_graph(history_list)

                return data
            else:
                self.info_label.setText("Failed in fetching data")
        except Exception as e:
            self.info_label.setText(f"Error: {str(e)}")

    def draw_graph(self, history):
        try:
            self.y_data = []
            hour_labels = []

            for point in history:
                timestamp = point["timestamp"]
                dt = datetime.fromisoformat(timestamp.replace('Z', '+00:00'))
                hour_label = dt.strftime("%H:00")
                self.y_data.append(point["price"])
                if hour_label not in hour_labels:
                    hour_labels.append(hour_label)
                else:
                    hour_labels.append("                                                                              ")

            # Generate x_data as numeric indices
            self.x_data = list(range(len(self.y_data)))

            # Map numeric index to hour label
            ticks = [(i, hour_labels[i]) for i in range(len(hour_labels))]

            self.plot_data.setData(self.x_data, self.y_data)

            # Apply clean, hourly tick labels
            x_axis = self.plot_widget.getAxis("bottom")
            x_axis.setTicks([ticks])
            x_axis.setStyle(tickFont=QFont("Arial", 10), tickTextOffset=10)

        except Exception as e:
            print(f"Graph draw error: {e}")

    def open_buy(self):
        try:
            self.buy_window = BuySellWindow("Buy", self.currency_data, self.currency_id)
            self.buy_window.show()
        except Exception:
            x = traceback.format_exc()
            print(x)
            QMessageBox.critical(self, "ERROR", x)

    def open_ai_chat(self):
        self.ai_window = AIChatWindow()
        self.ai_window.show()

    def open_sell(self):
        self.sell_window = BuySellWindow("Sell", self.currency_data, self.currency_id)
        self.sell_window.show()

    # def go_back(self):
    #     self.select_window = CurrencySelectionWindow()
    #     self.select_window.show()
    #     self.close()
