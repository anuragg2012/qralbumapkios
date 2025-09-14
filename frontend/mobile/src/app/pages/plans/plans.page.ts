import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonSegment,
  IonSegmentButton,
  IonLabel,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardContent
} from '@ionic/angular/standalone';

interface Plan {
  name: string;
  monthly: number;
  yearly: number;
  features: string[];
  isFree: boolean;
}

@Component({
  selector: 'app-plans',
  templateUrl: './plans.page.html',
  styleUrls: ['./plans.page.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonSegment,
    IonSegmentButton,
    IonLabel,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent
  ]
})
export class PlansPage {
  period: 'monthly' | 'yearly' = 'monthly';
  plans: Plan[] = [
    {
      name: 'Free',
      monthly: 0,
      yearly: 0,
      features: ['Basic usage', 'Limited storage'],
      isFree: true
    },
    {
      name: 'Pro',
      monthly: 9.99,
      yearly: 99.99,
      features: ['Unlimited albums', 'HD uploads', 'Priority support'],
      isFree: false
    }
  ];
}
