export const metadata = {
  title: "Guardians Of The North — Backend",
  description: "API service for auth and save slots · Guardians Of The North",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
}
