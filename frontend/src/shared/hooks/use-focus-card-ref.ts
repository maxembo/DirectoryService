import { useCallback } from "react";

export function useFocusCardRef() {
	return useCallback((element: HTMLDivElement | null) => {
		if (!element) return;

		element.focus({ preventScroll: true });
		element.scrollIntoView({ behavior: "smooth", block: "center" });
	}, []);
}
