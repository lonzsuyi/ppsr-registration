"use client";

import Link from "next/link";

export default function Home() {
  return (
    <main className="max-w-xl mx-auto py-10 px-4">
      <h1 className="text-2xl font-bold mb-4">
        PPSR Batch Registration Upload Navigation
      </h1>
      <div className="text-center mb-14">
        <Link
          href="/upload/file"
          className="mt-4 text-gray-400 underline text-3xl"
        >
           To Upload File  》
        </Link>
      </div>
      <div className="text-center mb-14">
        <Link
          href="/upload/bigfile"
          className="mt-4 text-gray-400 underline text-3xl"
        >
          To Upload Big File  》
        </Link>
      </div>
    </main>
  );
}
