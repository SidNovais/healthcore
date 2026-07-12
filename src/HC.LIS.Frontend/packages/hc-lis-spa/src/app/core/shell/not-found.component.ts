import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HcButton } from '../../ui/button/button';
import { HcCard } from '../../ui/card/card';
import { MOTION, useMotion } from '../../ui/motion/motion';
import { gsap } from 'gsap';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink, HcButton, HcCard],
  templateUrl: './not-found.component.html',
  styleUrl: './not-found.component.css',
})
export class NotFoundComponent {
  constructor() {
    useMotion(ctx => {
      if (ctx.reduceMotion) return;
      gsap.from('.error-card', { autoAlpha: 0, y: 10, duration: MOTION.slow });
    });
  }
}
