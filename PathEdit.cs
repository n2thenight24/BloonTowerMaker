﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Towers;
using Assets.Scripts.Models.Towers.Behaviors.Attack;
using Assets.Scripts.Models.Towers.Projectiles;
using Assets.Scripts.Models.Towers.Weapons;
using Assets.Scripts.Simulation.Towers;
using BloonTowerMaker.Data;
using BloonTowerMaker.Logic;
using BloonTowerMaker.Properties;
using BTD_Mod_Helper.Api.Towers;

namespace BloonTowerMaker
{
    public partial class PathEdit : Form
    {
        string path;
        bool isBase = false;
        string lastImage = "";

        private ModelToList<TowerModel> pathModel;
        private ModelToList<AttackModel> attackModel;
        private ModelToList<WeaponModel> weaponModel;
        private ModelToList<ModTower> baseModel;
        private ModelToList<ModUpgrade> upgradeModel;
        private Dictionary<string, List<string>> selectedProjectiles = new Dictionary<string, List<string>>();
        private Textures textures;
        public PathEdit(string path = "000")
        {
            InitializeComponent();
            this.path = path;
            isBase = path == Resources.Base;
        }

        private void PathEdit_Load(object sender, EventArgs e)
        {
            //Load all models to their grids
            if (isBase)
            {
                //Load the 000 model
                baseModel = new ModelToList<ModTower>(Path.Combine(Project.instance.projectPath, Models.ParsePath(path), Resources.TowerPathJsonFile));
                dataGridPathMain.DataSource = baseModel.data.ToDataTable();
            } else
            {
                //Load the path model (not 000)
                upgradeModel = new ModelToList<ModUpgrade>(Path.Combine(Project.instance.projectPath, Models.ParsePath(path), Resources.TowerPathJsonFile));
                dataGridPathMain.DataSource = upgradeModel.data.ToDataTable();
            }

            //Load TowerModel  (function parameter)
            pathModel = new ModelToList<TowerModel>(Path.Combine(Project.instance.projectPath, Models.ParsePath(path), Resources.TowerModelJsonFile));
            dataGridPathProperty.DataSource = pathModel.data.ToDataTable();

            //Load AttackModel (from GetAttackModel())
            attackModel = new ModelToList<AttackModel>(Path.Combine(Project.instance.projectPath,Models.ParsePath(path),Resources.TowerAttackJsonFile));
            dataGridPathAttack.DataSource = attackModel.data.ToDataTable();

            //Load weapon model from weapons or GetWeapon()
            weaponModel = new ModelToList<WeaponModel>(Path.Combine(Project.instance.projectPath, Models.ParsePath(path), Resources.TowerGlobalAttackJsonFile));
            dataGridGlobalAttack.DataSource = weaponModel.data.ToDataTable();

            //Load all projectiles to list
            List<string> projectileNames = new List<string>();
            foreach (var file in Directory.GetFiles(Path.Combine(Project.instance.projectPath, Resources.ProjectileFolder),"*.json"))
            {
                ModelToList<WeaponModel> weapon = new ModelToList<WeaponModel>(file);
                projectileNames.Add(weapon.FindValue("name"));
            }
            dataGridProjectiles.DataSource = projectileNames.ToDataTableWithCheckbox();



            //Load selected projectiles
            selectedProjectiles= selectedProjectiles.loadSelected();
            foreach (DataGridViewRow row in dataGridProjectiles.Rows)
            {
                if (row.Cells[1].ValueType == typeof(bool) && selectedProjectiles[row.Cells[0].Value.ToString()].Contains(path))
                    row.Cells[1].Value = true;
            }

            //Display types load
            var monkeyTypes = typeof(TowerType).GetProperties();
            foreach (var propertyInfo in monkeyTypes)
            {
                if (propertyInfo.PropertyType.Name != nameof(String)) continue;
                combo_basemodel.Items.Add(propertyInfo.Name);
            }

            //Misc
            this.Text = $"Path: {path}"; //get tower path from calling button
            UpdateImages(); //Update images on form

            //Load display properties
            textures = new Textures(Path.Combine(Project.instance.projectPath, Models.ParsePath(path),
                Resources.TowerTexturesJsonFile));
            if (textures.dataDictionary.Count != 0)
            {
                combo_basemodel.SelectedItem = textures.dataDictionary.First().Key;
                var values = new[]
                {
                    textures.dataDictionary.First().Value[0], 
                    textures.dataDictionary.First().Value[1],
                    textures.dataDictionary.First().Value[2]
                };
                number_base1.Value = values[0];
                number_base2.Value = values[1];
                number_base3.Value = values[2];
            }
        }

        private void UpdateImages()
        {
            img_icon.Image?.Dispose();
            img_display.Image?.Dispose();
            img_display.Image = SelectImage.GetImage(SelectImage.image_type.PORTRAIT, path);
            img_icon.Image = SelectImage.GetImage(SelectImage.image_type.ICON, path);
            img_texture.Image = SelectImage.GetImage(SelectImage.image_type.DISPLAY, path);
        }
        private void button_ok_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void PathEdit_FormClosing(object sender, FormClosingEventArgs e)
        {
            //pathModel.data.UpdateFromDataTable(dataGridPathProperty.DataSource as DataTable);

            //If not base update the tier and path
            if (!isBase)
            {
                upgradeModel.Edit("Tier", Models.GetPathTier(path).ToString());
                upgradeModel.Edit("Path", Models.GetPathRow(path));
            }
            MainForm.ActiveForm.Update();
        }


        private void RemoveImage(string path)
        {
            var imageName = pathModel.Find("name")[2];
            if (string.IsNullOrWhiteSpace(imageName))
            {
                MessageBox.Show("Path name cannot be empty to remove an image");
                return;
            }
            try
            {
                File.Delete(Path.Combine(Project.instance.projectPath,Models.ParsePath(path),$"{imageName}{lastImage}.png"));
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Cant delete Image, Try to re-open path");
            }
            UpdateImages();
        }

        private void image_select_dialog_FileOk(object sender, CancelEventArgs e)
        {
            var imageName = pathModel.Find("name")[2];
            if (string.IsNullOrWhiteSpace(imageName))
            {
                MessageBox.Show("Path name cannot be empty to set an image");
                return;
            }
            var file = image_select_dialog.FileName;
            try
            {
                var new_filename = Path.Combine(Project.instance.projectPath, Models.ParsePath(path),
                    $"{imageName}{lastImage}.png");
                File.Copy(file,new_filename , true);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString(), "Error getting image", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            UpdateImages(); //Update images on form

        }

        private void PathEdit_Enter(object sender, EventArgs e)
        {
            PathEdit_Load(sender, e);
        }

        //Image Select
        private void img_display_MouseClick(object sender, MouseEventArgs e)
        {
            lastImage = "-Portrait";
            if (e.Button == MouseButtons.Right)
            {
                img_display.Image?.Dispose();
                img_display.Image = null;
                RemoveImage(path);
                return;
            }
            image_select_dialog.ShowDialog();
        }
        private void img_icon_MouseClick(object sender, MouseEventArgs e)
        {
            lastImage = "-Icon";
            if (e.Button == MouseButtons.Right)
            {
                img_icon.Image?.Dispose();
                img_display.Image = null;
                RemoveImage(path);
                return;
            }
            image_select_dialog.ShowDialog();
        }

        private void img_texture_MouseClick(object sender, MouseEventArgs e)
        {
            lastImage = "-Display";
            if (e.Button == MouseButtons.Right)
            {
                img_texture.Image?.Dispose();
                img_display.Image = null;
                RemoveImage(path);
                return;
            }
            image_select_dialog.ShowDialog();
        }
        private void dataGridPathProperty_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            pathModel.data.UpdateFromDataTable(dataGridPathProperty.DataSource as DataTable);
            pathModel.Save();
        }

        private void dataGridPathAttack_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            attackModel.data.UpdateFromDataTable(dataGridPathAttack.DataSource as DataTable);
            attackModel.Save();
        }

        private void dataGridProjectiles_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var value = dataGridProjectiles[e.ColumnIndex,e.RowIndex].Value as bool?; //get checkbox value
            if (value == null || value.GetType() != typeof(bool)) return;
            var name = dataGridProjectiles[e.ColumnIndex-1, e.RowIndex].Value.ToString(); //get checkbox name

            //if checkbox true
            if ((bool)value && !selectedProjectiles[name].Contains(path))
                selectedProjectiles[name].Add(path);
            
            //if checkbox false
            if (!(bool)value)    
                selectedProjectiles[name].Remove(path);
            //save projectiles
            selectedProjectiles.saveSelected();

        }

        private void combo_basemodel_SelectedIndexChanged(object sender, EventArgs e)
        {
            var prevKey = textures.dataDictionary.First().Key;
            textures.dataDictionary.RenameKey(prevKey,combo_basemodel.SelectedItem.ToString());
            textures.Save();
        }

        private void dataGridGlobalAttack_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            weaponModel.data.UpdateFromDataTable(dataGridGlobalAttack.DataSource as DataTable);
            weaponModel.Save();
        }

        private void dataGridPathMain_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (isBase)
            {
                baseModel.data.UpdateFromDataTable(dataGridPathMain.DataSource as DataTable);
                baseModel.Save();
            }
            else
            {
                upgradeModel.data.UpdateFromDataTable(dataGridPathMain.DataSource as DataTable);
                upgradeModel.Save();
            }
        }

        private void number_base1_ValueChanged(object sender, EventArgs e)
        {
            textures.dataDictionary.First().Value[0] = (int) number_base1.Value;
            textures.Save();
        }

        private void number_base2_ValueChanged(object sender, EventArgs e)
        {
            textures.dataDictionary.First().Value[1] = (int)number_base2.Value;
            textures.Save();
        }

        private void number_base3_ValueChanged(object sender, EventArgs e)
        {
            textures.dataDictionary.First().Value[2] = (int)number_base3.Value;
            textures.Save();
        }
    }
}
