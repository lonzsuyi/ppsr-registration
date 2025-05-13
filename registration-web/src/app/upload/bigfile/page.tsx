"use client";

import { useEffect, useState } from "react";
import Link from "next/link";

interface TaskStatus {
  taskId: string;
  status: string;
  submitted: number;
  processed: number;
  invalid: number;
  added: number;
  updated: number;
  completedAt?: string;
  error?: string;
}

export default function BigFileUpload() {
  const [file, setFile] = useState<File | null>(null);
  const [taskId, setTaskId] = useState<string | null>(null);
  const [status, setStatus] = useState<TaskStatus | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [history, setHistory] = useState<TaskStatus[]>(() => {
    if (typeof window !== "undefined") {
      const saved = localStorage.getItem("uploadHistory");
      return saved ? JSON.parse(saved) : [];
    }
    return [];
  });

  const saveHistory = (newHistory: TaskStatus[]) => {
    setHistory(newHistory);
    localStorage.setItem("uploadHistory", JSON.stringify(newHistory));
  };

  const handleUpload = async () => {
    if (!file) {
      setError("Please select a file");
      return;
    }

    const formData = new FormData();
    formData.append("file", file);

    setError(null);
    setTaskId(null);
    setStatus(null);
    setLoading(true);

    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api/batch/uploadbigfile`,
        {
          method: "POST",
          body: formData,
        }
      );

      const data = await res.json();
      if (!res.ok) {
        setError(data.message || "Upload failed");
      } else {
        setTaskId(data.taskId);
      }
    } catch (err: Error | unknown) {
      setError(err instanceof Error ? err.message : "An error occurred");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (!taskId) return;

    const interval = setInterval(async () => {
      try {
        const res = await fetch(
          `${process.env.NEXT_PUBLIC_API_URL}/api/batch/getstatus/${taskId}`
        );
        const data = await res.json();
        setStatus(data);

        if (data.status === "Completed" || data.status === "Failed") {
          clearInterval(interval);
          const updatedHistory = [
            data,
            ...history.filter((h) => h.taskId !== data.taskId),
          ].slice(0, 10);
          saveHistory(updatedHistory);
        }
      } catch {
        clearInterval(interval);
        setError("Failed to check status");
      }
    }, 2000);

    return () => clearInterval(interval);
  }, [taskId, history]);

  return (
    <main className="max-w-2xl mx-auto p-4 space-y-6">
      <div className="text-center mb-14">
        <Link
          href="/upload/file"
          className="mt-4 text-gray-400 underline text-3xl"
        >
          To Upload File 》
        </Link>
      </div>
      <h1 className="text-2xl font-bold">
        PPSR Big File Batch Upload Registration
      </h1>

      <label htmlFor="file-upload" className="sr-only">
        Upload
      </label>
      <input
        id="file-upload"
        data-testid="upload-submit-btn"
        type="file"
        accept=".csv"
        onChange={(e) => setFile(e.target.files?.[0] ?? null)}
        className="border rounded p-2 mr-4"
      />

      <button
        data-testid="upload-button"
        onClick={handleUpload}
        disabled={loading}
        className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700 disabled:opacity-50"
      >
        {loading ? "Uploading..." : "Upload"}
      </button>

      {error && <p className="text-red-600">❌ {error}</p>}

      {status && (
        <div className="mt-4 border p-4 rounded shadow">
          <p className="font-semibold">
            Status:{" "}
            <span
              className={
                status.status === "Completed"
                  ? "text-green-600"
                  : status.status === "Failed"
                  ? "text-red-600"
                  : "text-yellow-600"
              }
            >
              {status.status}
            </span>
          </p>

          {status.status === "Processing" && status.submitted > 0 && (
            <div className="mt-2">
              <div className="text-sm text-gray-600 mb-1">
                Progress: {status.processed} / {status.submitted}
              </div>
              <div className="w-full bg-gray-200 rounded h-2">
                <div
                  className="bg-blue-600 h-2 rounded transition-all duration-500"
                  style={{
                    width: `${(status.processed / status.submitted) * 100}%`,
                  }}
                />
              </div>
            </div>
          )}

          {status.status === "Completed" && (
            <ul className="mt-2 list-disc list-inside">
              <li>Submitted: {status.submitted}</li>
              <li>Processed: {status.processed}</li>
              <li>Invalid: {status.invalid}</li>
              <li>Added: {status.added}</li>
              <li>Updated: {status.updated}</li>
            </ul>
          )}

          {status.status === "Failed" && (
            <p className="text-red-600">Error: {status.error}</p>
          )}
        </div>
      )}

      {history.length > 0 && (
        <div className="mt-6">
          <div className="flex justify-between items-center mb-2">
            <h2 className="text-xl font-semibold">Upload History</h2>
            <button
              onClick={() => saveHistory([])}
              className="text-sm text-red-500 hover:underline"
            >
              Clear History
            </button>
          </div>

          <ul className="space-y-2">
            {history.map((item) => (
              <li key={item.taskId} className="border p-3 rounded">
                <div className="flex justify-between">
                  <span
                    className={
                      item.status === "Completed"
                        ? "text-green-600 font-medium"
                        : item.status === "Failed"
                        ? "text-red-600 font-medium"
                        : "text-yellow-600 font-medium"
                    }
                  >
                    {item.status}
                  </span>
                  <span className="text-sm text-gray-500">
                    {item.completedAt
                      ? new Date(item.completedAt).toLocaleString()
                      : "-"}
                  </span>
                </div>
                {item.status === "Completed" && (
                  <div className="text-sm text-gray-700">
                    Processed {item.processed} / {item.submitted}, Invalid:{" "}
                    {item.invalid}, Added: {item.added}, Updated: {item.updated}
                  </div>
                )}
                {item.status === "Failed" && (
                  <p className="text-sm text-red-600">Error: {item.error}</p>
                )}
              </li>
            ))}
          </ul>
        </div>
      )}
    </main>
  );
}
