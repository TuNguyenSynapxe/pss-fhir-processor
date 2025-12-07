import { useCallback, RefObject } from 'react';

interface ScrollToNodeOptions {
  behavior?: ScrollBehavior;
  block?: ScrollLogicalPosition;
  highlightDuration?: number;
}

/**
 * Custom hook to scroll to and highlight a tree node
 * @param containerRef Reference to the tree container element
 * @returns Function to scroll to a specific node
 */
export function useScrollToTreeNode(containerRef: RefObject<HTMLElement | null>) {
  const scrollToNode = useCallback(
    (nodeKey: string, options: ScrollToNodeOptions = {}) => {
      const {
        behavior = 'smooth',
        block = 'center',
        highlightDuration = 1200,
      } = options;

      if (!containerRef.current) return;

      // Find the target node in the tree
      const selector = `[data-tree-key="${nodeKey}"], .ant-tree-node-content-wrapper[data-key="${nodeKey}"]`;
      const targetNode = containerRef.current.querySelector(selector);

      if (targetNode) {
        // Scroll to the node
        (targetNode as HTMLElement).scrollIntoView({ behavior, block });

        // Add highlight class
        targetNode.classList.add('tree-node-highlight');

        // Remove highlight after duration
        setTimeout(() => {
          targetNode.classList.remove('tree-node-highlight');
        }, highlightDuration);
      }
    },
    [containerRef]
  );

  return scrollToNode;
}
