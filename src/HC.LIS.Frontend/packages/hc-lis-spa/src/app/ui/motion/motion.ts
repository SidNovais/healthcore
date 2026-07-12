import { DestroyRef, ElementRef, afterNextRender, inject } from '@angular/core';
import { gsap } from 'gsap';

/** Motion duration tokens in seconds — mirrors --motion-* in styles.css. */
export const MOTION = {
  fast: 0.15,
  normal: 0.2,
  slow: 0.3,
} as const;

export interface MotionContext {
  /** True when the user has prefers-reduced-motion: reduce — skip or zero-out animations. */
  reduceMotion: boolean;
}

export type MotionBuilder = (ctx: MotionContext) => void;

let defaultsApplied = false;

/** Applies project-wide gsap defaults once. Safe to call from every component. */
export function initMotionDefaults(): void {
  if (defaultsApplied) {
    return;
  }
  defaultsApplied = true;
  gsap.defaults({ duration: MOTION.normal, ease: 'power2.out' });
}

/**
 * Runs `build` inside a gsap.matchMedia scoped to `scope` (selector text only matches
 * descendants). The builder always runs; check ctx.reduceMotion to skip or zero-out
 * animations. Call revert() on the returned MatchMedia to kill everything it created
 * and restore inline styles.
 */
export function withMotion(scope: Element, build: MotionBuilder): gsap.MatchMedia {
  initMotionDefaults();
  const mm = gsap.matchMedia(scope);
  mm.add(
    {
      reduceMotion: '(prefers-reduced-motion: reduce)',
      allowMotion: '(prefers-reduced-motion: no-preference)',
    },
    context => {
      const conditions = context.conditions as { reduceMotion: boolean };
      build({ reduceMotion: conditions.reduceMotion });
    },
  );
  return mm;
}

/**
 * Angular component helper — call from the constructor (injection context).
 * Runs `build` after the first render, scoped to the host element, and reverts
 * everything the builder created when the component is destroyed.
 */
export function useMotion(build: MotionBuilder): void {
  const host = inject(ElementRef).nativeElement as Element;
  const destroyRef = inject(DestroyRef);
  let mm: gsap.MatchMedia | undefined;

  afterNextRender(() => {
    mm = withMotion(host, build);
  });

  destroyRef.onDestroy(() => mm?.revert());
}
