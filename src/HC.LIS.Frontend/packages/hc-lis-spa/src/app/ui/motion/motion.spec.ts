import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { gsap } from 'gsap';
import { MOTION, initMotionDefaults, useMotion, withMotion } from './motion';

function stubMatchMedia(reduce: boolean): void {
  Object.defineProperty(window, 'matchMedia', {
    writable: true,
    configurable: true,
    value: (query: string): MediaQueryList =>
      ({
        matches: query.includes('prefers-reduced-motion: reduce')
          ? reduce
          : query.includes('no-preference')
            ? !reduce
            : false,
        media: query,
        onchange: null,
        addListener: () => undefined,
        removeListener: () => undefined,
        addEventListener: () => undefined,
        removeEventListener: () => undefined,
        dispatchEvent: () => false,
      }) as unknown as MediaQueryList,
  });
}

describe('MOTION tokens', () => {
  it('mirrors the CSS motion tokens in seconds', () => {
    expect(MOTION.fast).toBe(0.15);
    expect(MOTION.normal).toBe(0.2);
    expect(MOTION.slow).toBe(0.3);
  });
});

describe('initMotionDefaults', () => {
  it('sets project-wide gsap defaults: 200ms, power2.out', () => {
    initMotionDefaults();

    const defaults = gsap.defaults() as { duration?: number; ease?: string };
    expect(defaults.duration).toBe(0.2);
    expect(defaults.ease).toBe('power2.out');
  });
});

describe('withMotion', () => {
  let scope: HTMLElement;

  beforeEach(() => {
    scope = document.createElement('div');
    scope.innerHTML = '<div class="target"></div>';
    document.body.appendChild(scope);
  });

  afterEach(() => {
    scope.remove();
  });

  it('invokes the builder with reduceMotion=false when the user allows motion', () => {
    stubMatchMedia(false);
    let received: { reduceMotion: boolean } | null = null;

    const mm = withMotion(scope, ctx => {
      received = ctx;
    });

    expect(received).toEqual({ reduceMotion: false });
    mm.revert();
  });

  it('invokes the builder with reduceMotion=true when prefers-reduced-motion is set', () => {
    stubMatchMedia(true);
    let received: { reduceMotion: boolean } | null = null;

    const mm = withMotion(scope, ctx => {
      received = ctx;
    });

    expect(received).toEqual({ reduceMotion: true });
    mm.revert();
  });

  it('scopes selector text to the given element and reverts inline styles on revert()', () => {
    stubMatchMedia(false);

    const mm = withMotion(scope, () => {
      gsap.set('.target', { x: 50 });
    });

    const target = scope.querySelector<HTMLElement>('.target')!;
    expect(target.style.transform).toContain('translate');

    mm.revert();
    expect(target.style.transform).toBe('');
  });
});

describe('useMotion', () => {
  @Component({
    selector: 'hc-motion-host',
    template: '<div class="target"></div>',
  })
  class MotionHostComponent {
    received: { reduceMotion: boolean } | null = null;

    constructor() {
      useMotion(ctx => {
        this.received = ctx;
        gsap.set('.target', { x: 25 });
      });
    }
  }

  it('runs the builder after the view renders', async () => {
    stubMatchMedia(false);
    const fixture = TestBed.createComponent(MotionHostComponent);

    expect(fixture.componentInstance.received).toBeNull();

    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.componentInstance.received).toEqual({ reduceMotion: false });
  });

  it('reverts all animations created in the builder when the component is destroyed', async () => {
    stubMatchMedia(false);
    const fixture = TestBed.createComponent(MotionHostComponent);
    fixture.detectChanges();
    await fixture.whenStable();

    const target = (fixture.nativeElement as HTMLElement).querySelector<HTMLElement>('.target')!;
    expect(target.style.transform).toContain('translate');

    fixture.destroy();
    expect(target.style.transform).toBe('');
  });
});
