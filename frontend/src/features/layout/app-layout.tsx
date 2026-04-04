"use client";

import { queryClient } from "@/shared/api/query-client";
import { SidebarProvider } from "@/shared/components/ui/sidebar";
import { TooltipProvider } from "@/shared/components/ui/tooltip";
import { QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "sonner";
import { Header } from "../header/header";
import { AppSidebar } from "../sidebar/app-sidebar";

export function Layout({
	children,
}: Readonly<{
	children: React.ReactNode;
}>) {
	return (
		<QueryClientProvider client={queryClient}>
			<SidebarProvider>
				<TooltipProvider>
					<div className="flex h-screen w-full">
						<AppSidebar />
						<div className="flex-1 flex flex-col min-w-0">
							<Header />
							<main className="flex-1 overflow-auto p-10">{children}</main>
							<Toaster
								position="top-center"
								duration={3000}
								richColors={true}
								theme="dark"
							/>
						</div>
					</div>
				</TooltipProvider>
			</SidebarProvider>
		</QueryClientProvider>
	);
}
