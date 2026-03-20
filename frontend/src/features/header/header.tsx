import { SidebarTrigger } from "@/shared/components/ui/sidebar";
import { routes } from "@/shared/routes";
import Link from "next/link";

export function Header() {
	return (
		<header className="sticky top-0 z-50 bg-blue-500">
			<div className="flex h-13 items-center gap-3 px-3">
				<SidebarTrigger className="md:hidden text-black" />

				<Link
					href={routes.home.href}
					className="flex items-center gap-3 rounded-lg px-2 py-1.5 hover:bg-slate-100 transition"
				>
					<div className="flex h-8 w-8 items-center justify-center rounded-lg bg-slate-900 text-xs font-semibold text-white">
						DS
					</div>

					<span className="text-sm font-semibold text-slate-900">
						{routes.home.title}
					</span>
				</Link>
			</div>
		</header>
	);
}