import { ChangeDetectionStrategy, Component, computed, input, signal } from '@angular/core';

/**
 * Derives at most two display letters from a name. Multi-word names give
 * first+last initials ("Ada Lovelace" -> "AL"); a single token — which is what
 * HC.LIS sessions carry, since userName is an email — gives its first two
 * characters ("itadmin@hclis.local" -> "IT").
 */
function initialsOf(name: string): string {
  const words = name.trim().split(/\s+/).filter(Boolean);
  if (words.length === 0) {
    return '?';
  }
  const letters =
    words.length > 1 ? words[0][0] + words[words.length - 1][0] : words[0].slice(0, 2);
  return letters.toUpperCase();
}

/**
 * Hand-rolled avatar blueprinted from the shadcn `avatar` anatomy (root + image +
 * fallback; shadcn ships on radix/React, so only the contract is portable). The
 * image is optional and falls back to initials — HC.LIS has no user photos today,
 * so the fallback is the live path.
 */
@Component({
  selector: 'hc-avatar',
  templateUrl: './avatar.html',
  styleUrl: './avatar.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    class: 'hc-avatar',
    '[style.--hc-avatar-size]': 'size() + "px"',
    '[attr.data-testid]': 'testId()',
  },
})
export class HcAvatar {
  readonly name = input.required<string>();
  readonly src = input<string | null>(null);
  readonly size = input(32);
  readonly testId = input('avatar');

  /** The src that failed to load, if any — tracked by value so a new src retries. */
  private readonly failedSrc = signal<string | null>(null);

  protected readonly showImage = computed(() => !!this.src() && this.src() !== this.failedSrc());
  protected readonly initials = computed(() => initialsOf(this.name()));

  protected onError(): void {
    this.failedSrc.set(this.src());
  }
}
