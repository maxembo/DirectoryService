import { usePathname, useRouter, useSearchParams } from "next/navigation";

export type ArchiveView = "active" | "archived";

export function useArchiveView() {
	const pathname = usePathname();
	const router = useRouter();
	const searchParams = useSearchParams();

	const view: ArchiveView =
		searchParams.get("view") === "archived" ? "archived" : "active";

	const setView = (nextView: ArchiveView) => {
		if (nextView === view) {
			return;
		}
		const params = new URLSearchParams(searchParams.toString());

		params.set("view", nextView);

		params.delete("focus");

		router.replace(`${pathname}?${params.toString()}`, {
			scroll: false,
		});
	};

	return { view, setView };
}
