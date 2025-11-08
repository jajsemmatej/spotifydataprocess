import pandas as pd
import matplotlib.pyplot as plt
import sys
from pathlib import Path
import os
import matplotlib.dates as mdates

def main():
    if len(sys.argv) < 2:
        print("Usage: python script.py <filename>")
        sys.exit(1)

    filename = sys.argv[1]
    json_path = None

    binFolder = Path("bin/Debug/net8.0")
    if binFolder.exists() and binFolder.is_dir():
        json_path = binFolder / f"{filename}.json"
    else:
        json_path = Path(f"{filename}.json")

    if not json_path.exists():
        print(f"Error: JSON file not found at {json_path}")
        sys.exit(1)

    graphsFolder = Path("graphs")
    if not graphsFolder.exists():
        os.makedirs(graphsFolder)

    df = pd.read_json(json_path)
    df["Date"] = pd.to_datetime(df["Date"])
    df = df.sort_values("Date")
    
    plt.figure(figsize=(10, 5))
    plt.plot(df["Date"], df["AvgPlaytime"], linewidth=2, marker="")
    plt.title("Average Playtime (Smoothed)")
    plt.xlabel("Date")
    plt.ylabel("Avg Playtime (ms)")
    plt.grid(True)
    plt.tight_layout()

    ax = plt.gca()
    ax.xaxis.set_major_locator(mdates.AutoDateLocator())
    ax.xaxis.set_major_formatter(mdates.ConciseDateFormatter(ax.xaxis.get_major_locator()))

    plt.savefig("graphs/" + filename + ".png")
    plt.close()


if __name__ == "__main__":
    main()