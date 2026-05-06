import { useCallback, useRef } from "react";

type Props = {
	hasNextPage: boolean;
	isFetchingNextPage: boolean;
	fetchNextPage: () => void;
};

export const useCursorRef = ({
	hasNextPage,
	isFetchingNextPage,
	fetchNextPage,
}: Props) => {
	const observerRef = useRef<IntersectionObserver>(null);

	return useCallback(
		(node: HTMLDivElement | null) => {
			observerRef.current?.disconnect();

			if (!node || !hasNextPage) {
				return;
			}

			observerRef.current = new IntersectionObserver(([entry]) => {
				if (entry.isIntersecting && !isFetchingNextPage) {
					fetchNextPage();
				}
			});

			observerRef.current.observe(node);
		},
		[hasNextPage, isFetchingNextPage, fetchNextPage],
	);
};
