import { Layout } from "@/features/layout/app-layout";
import { cn } from "@/shared/lib/utils";
import type { Metadata } from "next";
import { Geist, Geist_Mono, Inter } from "next/font/google";
import "./styles/globals.css";

const inter = Inter({ subsets: ["latin"], variable: "--font-sans" });

const geistSans = Geist({
	variable: "--font-geist-sans",
	subsets: ["latin"],
});

const geistMono = Geist_Mono({
	variable: "--font-geist-mono",
	subsets: ["latin"],
});

export const metadata: Metadata = {
	title: "Directory Service",
};

export default function RootLayout({
	children,
}: Readonly<{
	children: React.ReactNode;
}>) {
	return (
		<html lang="en" className={cn("font-sans dark", inter.variable)}>
			<body
				className={`${geistSans.variable} ${geistMono.variable} antialiased`}
			>
				<Layout>{children}</Layout>
			</body>
		</html>
	);
}
