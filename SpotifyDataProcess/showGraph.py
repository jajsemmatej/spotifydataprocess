import pandas as pd
import matplotlib.pyplot as plt

df = pd.read_json("bin/Debug/net8.0/graphData.json")
df["Date"] = pd.to_datetime(df["Date"])
df = df.sort_values("Date")

plt.figure(figsize=(10, 5))
plt.plot(df["Date"], df["AvgPlaytime"], marker="o", linewidth=2)
plt.title("Average Playtime (Smoothed)")
plt.xlabel("Date")
plt.ylabel("Avg Playtime (ms)")
plt.grid(True)
plt.tight_layout()
plt.show()