import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HcButton } from '../../ui/button/button';
import { HcCard } from '../../ui/card/card';
import { HcIcon } from '../../ui/icon/icon';
import { MOTION, useMotion } from '../../ui/motion/motion';
import { gsap } from 'gsap';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [RouterLink, HcButton, HcCard, HcIcon],
  templateUrl: './unauthorized.component.html',
  styleUrl: './unauthorized.component.css',
})
export class UnauthorizedComponent {
  constructor() {
    useMotion(ctx => {
      if (ctx.reduceMotion) return;
      gsap.from('.error-card', { autoAlpha: 0, y: 10, duration: MOTION.slow });
    });
  }
}
