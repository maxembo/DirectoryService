"use client";

import {
	Sidebar,
	SidebarContent,
	SidebarGroup,
	SidebarHeader,
	SidebarMenu,
	SidebarMenuButton,
	SidebarMenuItem,
	SidebarTrigger,
	useSidebar,
} from "@/shared/components/ui/sidebar";
import { routes } from "@/shared/routes";
import { Briefcase, Building2, FolderTree } from "lucide-react";
import Link from "next/link";
import { usePathname } from "next/navigation";

const menuItems = [
	{
		href: routes.locations.href,
		label: routes.locations.title,
		icon: Building2,
	},
	{
		href: routes.departments.href,
		label: routes.departments.title,
		icon: FolderTree,
	},
	{
		href: routes.positions.href,
		label: routes.positions.title,
		icon: Briefcase,
	},
];

export function AppSidebar() {
	const pathname = usePathname();
	const { setOpenMobile } = useSidebar();

	return (
		<Sidebar collapsible="icon">
			<SidebarHeader className="px-2 py-2.5">
				<SidebarTrigger />
			</SidebarHeader>

			<SidebarContent>
				<SidebarGroup>
					<SidebarMenu className="space-y-2">
						{menuItems.map((item) => {
							const isActive =
								pathname === item.href || pathname.startsWith(item.href + "/");

							return (
								<SidebarMenuItem key={item.href}>
									<SidebarMenuButton
										asChild
										isActive={isActive}
										tooltip={item.label}
										className="transition-colors hover:bg-blue-500"
										onClick={() => setOpenMobile(false)}
									>
										<Link href={item.href}>
											<item.icon className="h-5 w-5" />
											<span>{item.label}</span>
										</Link>
									</SidebarMenuButton>
								</SidebarMenuItem>
							);
						})}
					</SidebarMenu>
				</SidebarGroup>
			</SidebarContent>
		</Sidebar>
	);
}
