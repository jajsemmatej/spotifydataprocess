import pandas as pd
import matplotlib.pyplot as plt
import sys
from pathlib import Path
import os
import matplotlib.dates as mdates
import platform

def createCombinedGraph(json_path1, json_path2):
    # Load both JSON files
    df1 = pd.read_json(json_path1)
    df2 = pd.read_json(json_path2)

    # Ensure proper date formatting and sorting
    df1["Date"] = pd.to_datetime(df1["Date"])
    df2["Date"] = pd.to_datetime(df2["Date"])
    df1 = df1.sort_values("Date")
    df2 = df2.sort_values("Date")

    # Plot
    plt.figure(figsize=(10, 5))
    plt.plot(df1["Date"], df1["AvgPlaytime"], label=json_path1.stem, linewidth=2)
    plt.plot(df2["Date"], df2["AvgPlaytime"], label=json_path2.stem, linewidth=2)

    plt.title("Average Playtime Comparison")
    plt.xlabel("Date")
    plt.ylabel("Avg Playtime (ms)")
    plt.grid(True)
    plt.legend()
    plt.tight_layout()

    # Format x-axis dates
    ax = plt.gca()
    ax.xaxis.set_major_locator(mdates.AutoDateLocator())
    ax.xaxis.set_major_formatter(mdates.ConciseDateFormatter(ax.xaxis.get_major_locator()))

    plt.show()

def main():
    # Check for correct argument count
    if len(sys.argv) != 3:
        print("Usage: python compare_graphs.py <json_file1> <json_file2>")
        sys.exit(1)

    file1_name = sys.argv[1]
    file2_name = sys.argv[2]

    # Determine paths depending on platform
    binFolder = Path("bin/Debug/net8.0")
    if platform.system() == "Windows":
        json_folder = binFolder / "graph_data"
    else:
        json_folder = Path("graph_data")

    file1 = json_folder / f"{file1_name}.json"
    file2 = json_folder / f"{file2_name}.json"

    if not file1.exists() or not file2.exists():
        print("Error: One or both JSON files not found.")
        sys.exit(1)

    createCombinedGraph(file1, file2)

if __name__ == "__main__":
    main()