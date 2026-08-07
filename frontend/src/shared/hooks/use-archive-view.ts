import { usePathname, useRouter, useSearchParams } from "next/navigation";

export type ArchiveView = "active" | "archived";

export function useArchiveView() {
	const pathname = usePathname();
	const router = useRouter();
	const searchParams = useSearchParams();

	const view: ArchiveView =
		searchParams.get("view") === "archived" ? "archived" : "active";

	const setView = (nextView: ArchiveView) => {
		const params = new URLSearchParams(searchParams.toString());

		params.set("view", nextView);

		router.replace(`${pathname}?${params.toString()}`, {
			scroll: false,
		});
	};

	return { view, setView };
}
