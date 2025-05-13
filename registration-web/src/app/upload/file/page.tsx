"use client";

import { useState } from "react";
import Link from "next/link";

interface UploadResult {
  submittedRecords: number;
  invalidRecords: number;
  processedRecords: number;
  addedRecords: number;
  updatedRecords: number;
}

export default function FileUpload() {
  const [file, setFile] = useState<File | null>(null);
  const [result, setResult] = useState<UploadResult | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleUpload = async () => {
    if (!file) {
      setError("Please select a CSV file");
      return;
    }

    const formData = new FormData();
    formData.append("file", file);

    setLoading(true);
    setError(null);
    setSuccess(null);
    setResult(null);

    try {
      const res = await fetch(
        `${process.env.NEXT_PUBLIC_API_URL}/api/batch/upload`,
        {
          method: "POST",
          body: formData,
        }
      );

      interface UploadResult {
        submittedRecords: number;
        invalidRecords: number;
        processedRecords: number;
        addedRecords: number;
        updatedRecords: number;
      }

      let data: UploadResult | { detail?: string };
      try {
        data = await res.json();
      } catch {
        setError("Failed to parse server response.");
        return;
      }

      if (!res.ok) {
        const errorResponse = data as { detail?: string };
        setError(errorResponse.detail || "Unknown error");
      } else {
        const resultData = data as UploadResult;
        setResult(resultData);
        setSuccess(
          `Upload successful: ${resultData.processedRecords} records processed`
        );
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Upload failed");
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="max-w-2xl mx-auto p-4 space-y-6">
      <div className="text-center mb-14">
        <Link
          href="/upload/bigfile"
          className="mt-4 text-gray-400 underline text-3xl"
        >
          To Upload Big File 》
        </Link>
      </div>
      <h1 className="text-2xl font-bold mb-4">
        PPSR File Batch Upload Registration
      </h1>

      <input
        type="file"
        accept=".csv"
        onChange={(e) => setFile(e.target.files?.[0] ?? null)}
        className="mb-4 cursor-pointer border border-gray-300 rounded-md p-2 mr-2"
      />

      <button
        data-testid="upload-submit-btn"
        onClick={handleUpload}
        disabled={loading}
        className="bg-blue-600 text-white px-4 py-2 rounded"
      >
        {loading ? "Uploading..." : "Upload CSV"}
      </button>

      {error && <p className="mt-4 text-red-600">❌ {error}</p>}
      {success && <p className="mt-4 text-green-600">✅ {success}</p>}

      {result && (
        <div className="mt-6">
          <h2 className="text-xl font-semibold mb-2">Upload Result:</h2>
          <ul className="list-disc list-inside">
            <li>Submitted Total: {result.submittedRecords}</li>
            <li>Invalid Records: {result.invalidRecords}</li>
            <li>Processed Successfully: {result.processedRecords}</li>
            <li>Added Records: {result.addedRecords}</li>
            <li>Updated Records: {result.updatedRecords}</li>
          </ul>
        </div>
      )}
    </main>
  );
}
