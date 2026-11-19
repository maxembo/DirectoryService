import { usePathname, useRouter, useSearchParams } from "next/navigation";

export function useFocusedCard() {
	const router = useRouter();
	const pathname = usePathname();
	const searchParams = useSearchParams();
	const focusedId = searchParams.get("focus");

	const handleRemoveFocus = () => {
		const params = new URLSearchParams(searchParams.toString());

		params.delete("focus");

		router.replace(
			`${pathname}${params.size ? `?${params.toString()}` : ""}`,
			{},
		);
	};
	return { focusedId, handleRemoveFocus };
}
