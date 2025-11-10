import pandas as pd
import matplotlib.pyplot as plt
import sys
from pathlib import Path
import os
import matplotlib.dates as mdates
import platform

def createGraph(json_path, outfilename):
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

    plt.savefig("graphs/" + outfilename + ".png")
    plt.close()

def main():
    json_folder = None

    binFolder = Path("bin/Debug/net8.0")
    if platform.system() == "Windows":
        json_folder = binFolder / f"graph_data"
    else:
        json_folder = Path(f"graph_data")

    if not json_folder.exists():
        print(f"Error: graph_data folder not found")
        sys.exit(1)

    graphsFolder = Path("graphs")
    if not graphsFolder.exists():
        os.makedirs(graphsFolder)

    for file in json_folder.iterdir():
        if file.is_file() and file.suffix.lower() == ".json":
            filename = file.stem
            createGraph(file, filename)


if __name__ == "__main__":
    main()