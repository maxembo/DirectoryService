import {
	Pagination,
	PaginationContent,
	PaginationEllipsis,
	PaginationItem,
	PaginationLink,
	PaginationNext,
	PaginationPrevious,
} from "@/shared/components/ui/pagination";

type PaginationIconsOnlyProps = {
	currentPage: number;
	totalPages: number;
	onPageChange: (page: number) => void;
};

type PageToken = number | "ellipsis";

function buildPaginationItems(
	currentPage: number,
	totalPages: number,
): PageToken[] {
	if (totalPages <= 7) {
		return Array.from({ length: totalPages }, (_, i) => i + 1);
	}

	const items: PageToken[] = [1];
	const left = Math.max(currentPage - 1, 2);
	const right = Math.min(currentPage + 1, totalPages - 1);

	if (left > 2) {
		items.push("ellipsis");
	}

	for (let page = left; page <= right; page++) {
		items.push(page);
	}

	if (right < totalPages - 1) {
		items.push("ellipsis");
	}

	items.push(totalPages);

	return items;
}

export function PaginationIconsOnly({
	currentPage,
	totalPages,
	onPageChange,
}: PaginationIconsOnlyProps) {
	if (totalPages <= 1) return null;

	const pages = buildPaginationItems(currentPage, totalPages);

	const goToPage = (page: number) => {
		if (page < 1 || page > totalPages || page === currentPage) return;
		onPageChange(page);
	};

	return (
		<Pagination>
			<PaginationContent>
				<PaginationItem>
					<PaginationPrevious
						href="#"
						className={
							currentPage === 1 ? "pointer-events-none opacity-50" : ""
						}
						onClick={(e) => {
							e.preventDefault();
							goToPage(currentPage - 1);
						}}
					/>
				</PaginationItem>

				{pages.map((item, index) => {
					if (item === "ellipsis") {
						return (
							<PaginationItem key={`ellipsis-${index}`}>
								<PaginationEllipsis />
							</PaginationItem>
						);
					}

					return (
						<PaginationItem key={item}>
							<PaginationLink
								href="#"
								isActive={item === currentPage}
								onClick={(e) => {
									e.preventDefault();
									goToPage(item);
								}}
							>
								{item}
							</PaginationLink>
						</PaginationItem>
					);
				})}

				<PaginationItem>
					<PaginationNext
						href="#"
						className={
							currentPage === totalPages ? "pointer-events-none opacity-50" : ""
						}
						onClick={(e) => {
							e.preventDefault();
							goToPage(currentPage + 1);
						}}
					/>
				</PaginationItem>
			</PaginationContent>
		</Pagination>
	);
}
