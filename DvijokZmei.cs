using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace ZDll
{
	// класс, описывающий работу с координатами мышонка и секциями змеи
	public class koordinata
	{
		public int X;
		public int Y;
		public koordinata(int x, int y)
		{
			X = x; Y = y;
		}
	}

	public class Dvijok
	{
		public bool gamover;
		public int interval;
		public int mouses; // количество пойманых мышек
		public int Shag; // уровень игры
		public int ochki; // набранные очки в игре

		// описание змейки- список секций(нулевой индекс в списке - голова змеи) 
		Random rand = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
		List<koordinata> zmeyka = new List<koordinata>();
		koordinata mouse; // координаты мышонка
		int napravlenie; // направление движения змейки: 0 - вверх, 1 - вправо, 2 - вниз, 3 - влево
		int tmpH;

		int W, H, S;//задаем размер поля игры в клетках (ширина, высота), размер клетки в пикселях
		public Brush brsh1, brsh2, brsh3;

		//конструктор вызывается при создании объекта
		public Dvijok(int w, int h, int s, Bitmap bitmapSize1, Bitmap bitmapSize2, Bitmap bitmapSize3, int intrv)
		{
			W = w;
			H = h;
			S = s;
			brsh1 = new TextureBrush(bitmapSize1);
			brsh2 = new TextureBrush(bitmapSize2);
			brsh3 = new TextureBrush(bitmapSize3);
			interval = intrv;

			gamover = false;
		}

		//сброс необходимый в начале игры
		public void SbrosSchetchikov()
		{
			napravlenie = 0; // направление движения змейки: 0 - вверх, 1 - вправо, 2 - вниз, 3 - влево
			mouses = 0; // количество пойманых мышек
			Shag = 1; // уровень игры
			ochki = 0; // набранные очки в игре


			napravlenie = 0;
			// делаем змейку из трех секции, с начальными координатами внизу и по-центру формы
			// удалаем все звенья змейки. это нужно для перезапусков программы
			for (int i = zmeyka.Count(); i > 0; i--)
				zmeyka.Remove(zmeyka[i - 1]);

			zmeyka.Add(new koordinata(W / 2, H - 5));
			zmeyka.Add(new koordinata(W / 2, H - 4));
			zmeyka.Add(new koordinata(W / 2, H - 3));
			zmeyka.Add(new koordinata(W / 2, H - 2));
			zmeyka.Add(new koordinata(W / 2, H - 1));

			mouse = new koordinata(rand.Next(W), rand.Next(H)); // координаты мышонка
		}


		// меняем направление движения, если оно не противоположное
		public void RulimZmeei(KeyEventArgs e)
		{
			switch (e.KeyData)
			{
				case Keys.Up:
					if (napravlenie != 2)
						napravlenie = 0;
					break;
				case Keys.Right:
					if (napravlenie != 3)
						napravlenie = 1;
					break;
				case Keys.Down:
					if (napravlenie != 0)
						napravlenie = 2;
					break;
				case Keys.Left:
					if (napravlenie != 1)
						napravlenie = 3;
					break;
			}
		}

		//расчет движения змеи
		public void Dvijenie()
		{
			//ЗМЕЯ
			// запоминаем координаты головы змейки
			int x = zmeyka[0].X, y = zmeyka[0].Y;
			// в зависимости от направления вычисляем где будет голова на следующем шаге
			// сделал чтобы при достижении края формы голова появлялась с противоположной стороны 
			// и змея продолжала движение
			int visotaPaneliKnopok = 1;
			switch (napravlenie)
			{
				case 0:
					y--;
					if (y < (0 + visotaPaneliKnopok))
						y = H - 1 - visotaPaneliKnopok;
					break;
				case 1:
					x++;
					if (x >= W)
						x = 0;
					break;
				case 2:
					y++;
					if (y >= (H - visotaPaneliKnopok))
						y = 0 + visotaPaneliKnopok;
					break;
				case 3:
					x--;
					if (x < 0)
						x = W - 1;
					break;
			}
			koordinata c = new koordinata(x, y); // секция с новыми координатами головы
			zmeyka.Insert(0, c); // вставляем его в начало списка сегментов змеи(змея выросла на одну секцию)

			//проверим на седъедание самой себя
			//змея это массив. поэтому пробежимся по нему с целью узнать нет ли совпадения
			for (int i = 1; i < zmeyka.Count(); i++)
			{
				if (zmeyka[i].X == zmeyka[0].X && zmeyka[i].Y == zmeyka[0].Y)
					gamover = true;
			}



			//МЫШЬ
			if (zmeyka[0].X == mouse.X && zmeyka[0].Y == mouse.Y) // если координаты головы и мышки совпали
			{
				tmpH = rand.Next(H);
				if (tmpH == 0)
					tmpH = 1;
				if (tmpH >= H - 1)
					tmpH = tmpH - 3;
				mouse = new koordinata(rand.Next(W), tmpH); // располагаем мышонка в новых случайных координатах

				mouses++; // увеличиваем счетчик пойманых мышат
				ochki += Shag; // увеличиваем набранные очки в игре: за каждого мышонка прибавляем количество равное номеру уровня
				if (mouses % 10 == 0) // после каждого десятого мышонка
				{
					Shag++; // повышаем уровень
					interval -= 10; // и уменьшаем интервал срабатывания таймера
				}
			}
			else // если координаты головы и коодинаты мышонка не совпали - убираем последнюю секцию змейки(т.к. ранее добавляли новую голову)
				zmeyka.RemoveAt(zmeyka.Count - 1);
		}

		//рисуем
		public void Paint(PaintEventArgs e)
		{
			//рисуем мышку
			e.Graphics.FillEllipse(brsh1, new Rectangle(mouse.X * S, mouse.Y * S, S, S));

			//рисуем змейку
			e.Graphics.FillRectangle(brsh3, new Rectangle(zmeyka[0].X * S, zmeyka[0].Y * S, S, S));
			for (int i = 1; i < zmeyka.Count; i++)
				e.Graphics.FillRectangle(brsh2, new Rectangle(zmeyka[i].X * S, zmeyka[i].Y * S, S, S));
		}
	}
}
