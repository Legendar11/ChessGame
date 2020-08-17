using CoreGame;
using CoreGame.Enums;
using CoreGame.Extensions;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace UI
{
    public partial class FrmMain : Form
    {
        internal ChessGameHandler game = new ChessGameHandler(); // текущая игра

        public FrmMain()
        {
            InitializeComponent();
        }

        // отрисовка игрового поля
        private void PboxChess_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;

            // отрисовка всех игровых фигур
            foreach (var peice in game.InnerGame.GamePeices)
            {
                var img = game.GetPieceImage(peice.Figure, peice.Side);
                g.DrawImage(img, peice.Point.X * (PboxChess.Width / 8f), (7 - peice.Point.Y) * (PboxChess.Width / 8f));
            }

            // если какая-либо из фигур выбрана
            if (!(game.SelectedPeice is null))
            {
                // проверка, не под атакой ли король
                var kingIsUnderAttack = game.InnerGame.KingIsUnderAttck();
                if (kingIsUnderAttack) // если да
                {
                    var curMoves = game.InnerGame.WhoCanDestroyAttacker(); // ходы, которые помогут спасти короля

                    if (!curMoves.Any(x => x.Peice.Equals(game.SelectedPeice.Value))) // если выбрана не та фигура, которая может спасти короля
                    {
                        game.SelectedPeice = null;
                        return; // выходим
                    }

                    // определяем ходы для защиты
                    var movesForDefend = curMoves.Where(x => x.Peice.Equals(game.SelectedPeice.Value));
                    var imgPossiblePeicesForDefend = game.GetGameImage(ChessGameHandler.GameImageType.Selected);
                    foreach (var move in movesForDefend) // рисуем ходы для защиты выбранной фигуры
                    {
                        g.DrawImage(imgPossiblePeicesForDefend,
                             move.DestinationPoint.X * (PboxChess.Width / 8f), (7 - move.DestinationPoint.Y) * (PboxChess.Width / 8f));
                    }
                    return;
                }

                var moves = game.SelectedPeice.Value.GetMoves(game.InnerGame); // список доступных ходов для выбранной фигуры
                var imgPossiblePeices = game.GetGameImage(ChessGameHandler.GameImageType.Selected); // изображение доступных ходов
                foreach (var move in moves) // отрисовываем все ходы
                {
                    g.DrawImage(imgPossiblePeices,
                         move.DestinationPoint.X * (PboxChess.Width / 8f), (7 - move.DestinationPoint.Y) * (PboxChess.Width / 8f));
                }
            }
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            
        }

        // клик по игровому полю
        private void PboxChess_MouseClick(object sender, MouseEventArgs e)
        {
            var loc = new Point((int)Math.Floor((e.X / (float)PboxChess.Width) * 8), 7 - (int)Math.Floor((e.Y / (float)PboxChess.Width) * 8));

            var peice = game.GetPeiceOnLocation(loc); // клетка на игровом поле
            if (peice.HaveFigure) // если клетка имеет фигуру
            {
                if (peice.Side == game.InnerGame.Turn) // если наш ход
                {
                    game.SelectPeice(loc); // выбираем фигуру
                }
                else
                {
                    game.MakeMove(loc, checkBox1.Checked); // иначе делаем ход выбранной фигурой (атака)
                }
            }
            else
            {
                game.MakeMove(loc, checkBox1.Checked); // иначе делаем ход выбранной фигурой (передвижение)
            }

            if (game.Winner != CoreGame.Enums.GameSide.Undefined) // если определен победитель - высвечиывааем его
            {
                if (game.Winner == CoreGame.Enums.GameSide.Black)
                {
                    MessageBox.Show("Победили черные!");
                }
                if (game.Winner == CoreGame.Enums.GameSide.White)
                {
                    MessageBox.Show("Победили белые!");
                }

                // начинаем заново, сбрасывая все состояния
                game.Winner = CoreGame.Enums.GameSide.Undefined;
                game.InnerGame.Reset();
                var turn = game.InnerGame.Turn == GameSide.White ? GameSide.Black : GameSide.White;
                game.InnerGame = new ChessGame(turn, game.InnerGame.Board, game.InnerGame.GameMoves);
                return;
            }

            PboxChess.Invalidate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            game.Undo(); // отменяем ход черных
            game.Undo(); // отменяем ход белых 
            game.Winner = CoreGame.Enums.GameSide.Undefined;

            PboxChess.Invalidate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            game.InnerGame.Reset();
            game.SelectedPeice = null;
            PboxChess.Invalidate();
        }
    }
}
