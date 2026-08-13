import { cn } from "@/shared/lib/utils";
import { describe, expect, it } from "vitest";

describe("cn", () => {
	it("merges conditional and conflicting Tailwind classes", () => {
		expect(cn("px-2", false && "hidden", "px-4")).toBe("px-4");
	});
});
