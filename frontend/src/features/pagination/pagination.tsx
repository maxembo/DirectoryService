import {
	Pagination as PaginationComponent,
	PaginationContent,
	PaginationEllipsis,
	PaginationItem,
	PaginationLink,
	PaginationNext,
	PaginationPrevious,
} from "@/shared/components/ui/pagination";

type PaginationProps = {
	currentPage: number;
	totalPages: number;
	onPageChange: (page: number) => void;
	maxVisiblePages?: number;
};

type PageToken = number | "ellipsis";

function getVisiblePages(
	currentPage: number,
	totalPages: number,
	maxVisible = 7,
): PageToken[] {
	if (totalPages <= maxVisible)
		return Array.from({ length: totalPages }, (_, i) => i + 1);

	const items: PageToken[] = [1];
	const left = Math.max(currentPage - 1, 2);
	const right = Math.min(currentPage + 1, totalPages - 1);

	if (left > 2) items.push("ellipsis");
	for (let i = left; i <= right; i++) items.push(i);
	if (right < totalPages - 1) items.push("ellipsis");

	items.push(totalPages);
	return items;
}

export function Pagination({
	currentPage,
	totalPages,
	onPageChange,
	maxVisiblePages,
}: PaginationProps) {
	if (totalPages <= 1) return null;

	const pages = getVisiblePages(currentPage, totalPages, maxVisiblePages);

	const handlePageChange = (page: number) => {
		if (page < 1 || page > totalPages || page === currentPage) return;
		onPageChange(page);
	};

	return (
		<PaginationComponent>
			<PaginationContent>
				<PaginationItem>
					<PaginationPrevious
						className={
							currentPage === 1 ? "pointer-events-none opacity-50" : ""
						}
						onClick={() => handlePageChange(currentPage - 1)}
					/>
				</PaginationItem>

				{pages.map((item, index) =>
					item === "ellipsis" ? (
						<PaginationItem key={`ellipsis-${index}`}>
							<PaginationEllipsis />
						</PaginationItem>
					) : (
						<PaginationItem key={item}>
							<PaginationLink
								isActive={item === currentPage}
								onClick={() => handlePageChange(item)}
							>
								{item}
							</PaginationLink>
						</PaginationItem>
					),
				)}

				<PaginationItem>
					<PaginationNext
						className={
							currentPage === totalPages ? "pointer-events-none opacity-50" : ""
						}
						onClick={() => handlePageChange(currentPage + 1)}
					/>
				</PaginationItem>
			</PaginationContent>
		</PaginationComponent>
	);
}
