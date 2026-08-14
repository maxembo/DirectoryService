"use client";

import { queryClient } from "@/shared/api/query-client";
import { SidebarProvider } from "@/shared/components/ui/sidebar";
import { TooltipProvider } from "@/shared/components/ui/tooltip";
import { QueryClientProvider } from "@tanstack/react-query";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import { Toaster } from "sonner";
import { AppSidebar } from "./app-sidebar";
import { Header } from "./header";

export function AppShell({
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
						<div className="flex min-w-0 flex-1 flex-col">
							<Header />
							<main className="min-h-0 flex-1 overflow-auto p-10">
								{children}
							</main>
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
			<ReactQueryDevtools initialIsOpen={false} />
		</QueryClientProvider>
	);
}
